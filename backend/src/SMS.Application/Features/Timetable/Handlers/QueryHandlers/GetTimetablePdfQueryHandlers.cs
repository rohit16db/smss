using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Timetable.Queries;
using SMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SMS.Application.Features.Timetable.Handlers.QueryHandlers;

public class GetTimetablePdfQueryHandlers :
    IRequestHandler<GetSectionTimetablePdfQuery, byte[]>,
    IRequestHandler<GetStaffTimetablePdfQuery, byte[]>
{
    private readonly IApplicationDbContext _context;

    public GetTimetablePdfQueryHandlers(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> Handle(GetSectionTimetablePdfQuery request, CancellationToken cancellationToken)
    {
        var school = await _context.Schools.FirstOrDefaultAsync(s => s.IsActive, cancellationToken);
        var academicYear = await _context.AcademicYears.FirstOrDefaultAsync(y => y.Id == request.AcademicYearId, cancellationToken);
        var section = await _context.Sections.Include(s => s.Class).FirstOrDefaultAsync(s => s.Id == request.SectionId, cancellationToken);
        
        var slots = await _context.TimeSlots
            .Where(t => t.AcademicYearId == request.AcademicYearId)
            .OrderBy(t => t.StartTime)
            .ToListAsync(cancellationToken);

        var entries = await _context.TimetableEntries
            .Include(t => t.TimeSlot)
            .Include(t => t.StaffAssignment).ThenInclude(a => a!.Staff)
            .Include(t => t.StaffAssignment).ThenInclude(a => a!.Subject)
            .Where(t => t.AcademicYearId == request.AcademicYearId && t.StaffAssignment!.SectionId == request.SectionId)
            .ToListAsync(cancellationToken);

        var title = $"Timetable: {section?.Class?.Name} - {section?.SectionName}";
        return GeneratePdf(school, academicYear?.Name ?? "", title, slots, entries, true);
    }

    public async Task<byte[]> Handle(GetStaffTimetablePdfQuery request, CancellationToken cancellationToken)
    {
        var school = await _context.Schools.FirstOrDefaultAsync(s => s.IsActive, cancellationToken);
        var academicYear = await _context.AcademicYears.FirstOrDefaultAsync(y => y.Id == request.AcademicYearId, cancellationToken);
        var staff = await _context.Staff.FirstOrDefaultAsync(s => s.Id == request.StaffId, cancellationToken);
        
        var slots = await _context.TimeSlots
            .Where(t => t.AcademicYearId == request.AcademicYearId)
            .OrderBy(t => t.StartTime)
            .ToListAsync(cancellationToken);

        var entries = await _context.TimetableEntries
            .Include(t => t.TimeSlot)
            .Include(t => t.StaffAssignment).ThenInclude(a => a!.Section)
            .Include(t => t.StaffAssignment).ThenInclude(a => a!.Class)
            .Include(t => t.StaffAssignment).ThenInclude(a => a!.Subject)
            .Where(t => t.AcademicYearId == request.AcademicYearId && t.StaffAssignment!.StaffId == request.StaffId)
            .ToListAsync(cancellationToken);

        var title = $"Timetable: {staff?.FullName ?? "Staff Member"}";
        return GeneratePdf(school, academicYear?.Name ?? "", title, slots, entries, false);
    }

    private byte[] GeneratePdf(School? school, string academicYear, string title, List<TimeSlot> slots, List<TimetableEntry> entries, bool isSectionView)
    {
        var primaryColor = school?.PrimaryColor ?? "#1e3a8a";
        var secondaryColor = "#64748b";
        var accentColor = school?.AccentColor ?? "#3b82f6";
        var borderColor = "#cbd5e1";

        // Group slots by time (ignoring day) to create rows
        var slotGroups = slots.GroupBy(s => new { s.StartTime, s.EndTime, s.Name, s.IsBreak })
                             .OrderBy(g => g.Key.StartTime)
                             .ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, QuestPDF.Infrastructure.Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Verdana"));

                // Header
                page.Header().Column(headerCol =>
                {
                    headerCol.Item().Row(row =>
                    {
                        // Left: Logo
                        if (school?.LogoImage != null)
                        {
                            row.AutoItem().PaddingRight(15).Height(60).Image(school.LogoImage).FitHeight();
                        }
                        else
                        {
                            // Placeholder/Icon if no logo
                            row.AutoItem().PaddingRight(15).Height(60).AlignMiddle().Column(c => {
                                c.Item().Width(40).Height(40).Background(primaryColor).AlignCenter().AlignMiddle().Text("S").FontColor(Colors.White).FontSize(20).Bold();
                            });
                        }

                        // Middle: School Info
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(school?.Name?.ToUpper() ?? "SCHOOL MANAGEMENT SYSTEM").FontSize(22).Bold().FontColor(primaryColor).LetterSpacing(0.02f);
                            col.Item().Text(school?.Address ?? "").FontSize(9).FontColor(secondaryColor);
                            col.Item().Row(contactRow =>
                            {
                                contactRow.AutoItem().Text($"{school?.PhoneNumber}").FontSize(8).FontColor(secondaryColor);
                                if (!string.IsNullOrEmpty(school?.PhoneNumber) && !string.IsNullOrEmpty(school?.EmailAddress))
                                    contactRow.AutoItem().PaddingHorizontal(5).Text("|").FontSize(8).FontColor(secondaryColor);
                                contactRow.AutoItem().Text($"{school?.EmailAddress}").FontSize(8).FontColor(secondaryColor);
                            });
                            if (!string.IsNullOrEmpty(school?.Website))
                                col.Item().Text(school.Website).FontSize(8).Italic().FontColor(accentColor);
                        });

                        // Right: Timetable Info
                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("ACADEMIC TIMETABLE").FontSize(14).SemiBold().FontColor("#0f172a");
                            col.Item().Text($"Academic Session: {academicYear}").FontSize(9).FontColor(secondaryColor);
                            col.Item().PaddingTop(4).Text(title).FontSize(11).SemiBold().FontColor(accentColor);
                        });
                    });

                    headerCol.Item().PaddingVertical(10).LineHorizontal(1).LineColor("#e2e8f0");
                });

                // Content
                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(85);  // Time Slot
                        columns.RelativeColumn();    // Monday
                        columns.RelativeColumn();    // Tuesday
                        columns.RelativeColumn();    // Wednesday
                        columns.RelativeColumn();    // Thursday
                        columns.RelativeColumn();    // Friday
                        columns.RelativeColumn();    // Saturday
                    });

                    // Define generic cell styles
                    static IContainer HeaderStyle(IContainer container) => 
                        container.DefaultTextStyle(x => x.SemiBold().FontSize(8).FontColor(Colors.White))
                                 .Background("#1e3a8a") // Primary Blue
                                 .Border(0.5f)
                                 .BorderColor("#1e3a8a")
                                 .AlignCenter()
                                 .AlignMiddle()
                                 .PaddingVertical(8);

                    static IContainer BodyStyle(IContainer container) => 
                        container.Border(0.5f)
                                 .BorderColor("#cbd5e1") // Slate 300
                                 .Padding(4)
                                 .MinHeight(55)
                                 .AlignCenter()
                                 .AlignMiddle();

                    // Header Row
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderStyle).Text("TIME SLOT");
                        header.Cell().Element(HeaderStyle).Text("MONDAY");
                        header.Cell().Element(HeaderStyle).Text("TUESDAY");
                        header.Cell().Element(HeaderStyle).Text("WEDNESDAY");
                        header.Cell().Element(HeaderStyle).Text("THURSDAY");
                        header.Cell().Element(HeaderStyle).Text("FRIDAY");
                        header.Cell().Element(HeaderStyle).Text("SATURDAY");
                    });

                    // Rows
                    foreach (var group in slotGroups)
                    {
                        var isBreak = group.Key.IsBreak;

                        // Time Slot Column
                        table.Cell().Element(BodyStyle).Column(c =>
                        {
                            c.Item().Text(group.Key.Name).SemiBold().FontSize(8.5f).FontColor("#0f172a");
                            c.Item().Text($"{group.Key.StartTime:hh\\:mm} - {group.Key.EndTime:hh\\:mm}").FontSize(7.5f).FontColor("#64748b");
                            if (isBreak) c.Item().PaddingTop(2).Text("BREAK").FontSize(7).SemiBold().FontColor("#b45309");
                        });

                        // Days Columns
                        for (int day = 1; day <= 6; day++)
                        {
                            var slot = group.FirstOrDefault(s => (int)s.DayOfWeek == day);
                            var entry = slot != null ? entries.FirstOrDefault(e => e.TimeSlotId == slot.Id) : null;

                            var cell = table.Cell().Element(BodyStyle);
                            
                            if (isBreak)
                            {
                                cell.Background("#fef3c7"); // Light amber for breaks
                            }
                            else if (entry != null)
                            {
                                cell.Column(col =>
                                {
                                    col.Item().PaddingBottom(2).Text(entry.StaffAssignment?.Subject?.Name ?? "N/A").SemiBold().FontSize(9).FontColor("#1e40af").AlignCenter();
                                    
                                    if (isSectionView)
                                    {
                                        col.Item().Text(entry.StaffAssignment?.Staff?.FullName ?? "N/A").FontSize(8).Italic().FontColor("#475569").AlignCenter();
                                    }
                                    else
                                    {
                                        col.Item().Text($"{entry.StaffAssignment?.Class?.Name} - {entry.StaffAssignment?.Section?.SectionName}").FontSize(8).Italic().FontColor("#475569").AlignCenter();
                                    }

                                    if (!string.IsNullOrEmpty(entry.RoomNumber))
                                    {
                                        col.Item().PaddingTop(2).Text($"Room: {entry.RoomNumber}").FontSize(7).SemiBold().FontColor("#94a3b8").AlignCenter();
                                    }
                                });
                            }
                        }
                    }
                });

                // Footer
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated on ");
                    x.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm")).SemiBold();
                    x.Span(" | Page ");
                    x.CurrentPageNumber();
                });
            });
        });

        return document.GeneratePdf();
    }
}
