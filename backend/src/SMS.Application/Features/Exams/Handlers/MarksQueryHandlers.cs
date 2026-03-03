using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Exams.DTOs;
using SMS.Application.Features.Exams.Queries;

namespace SMS.Application.Features.Exams.Handlers;

public class MarksQueryHandlers
{
    public class GetMarksEntryFormQueryHandler : IRequestHandler<GetMarksEntryFormQuery, MarksEntryFormDto>
    {
        private readonly IApplicationDbContext _context;
        public GetMarksEntryFormQueryHandler(IApplicationDbContext context) => _context = context;
        
        public async Task<MarksEntryFormDto> Handle(GetMarksEntryFormQuery request, CancellationToken cancellationToken)
        {
            var exam = await _context.Exams
                .Include(e => e.ExamSubjects)
                .Include(e => e.ExamClasses)
                .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);

            if (exam == null)
                throw new InvalidOperationException($"Exam with ID {request.ExamId} not found");

            var subjects = await _context.ExamSubjects
                .Where(es => es.ExamId == request.ExamId)
                .Include(es => es.Subject)
                .Select(es => new { es.SubjectId, es.Subject!.Name, es.MaxMarks })
                .ToListAsync(cancellationToken);

            var examClass = await _context.ExamClasses
                .Include(ec => ec.Class)
                .FirstOrDefaultAsync(
                    ec => ec.ExamId == request.ExamId && ec.ClassId == request.ClassId,
                    cancellationToken);

            if (examClass == null)
                throw new InvalidOperationException("Selected class is not assigned to this exam");

            // Get the specific section
            var section = await _context.Sections
                .FirstOrDefaultAsync(s => s.Id == request.SectionId && s.ClassId == request.ClassId, cancellationToken);

            if (section == null)
                throw new InvalidOperationException("Selected section not found in this class");

            // Get students only from the selected section
            var studentSectionsQuery = _context.StudentSections
                .Where(ss => ss.SectionId == request.SectionId && ss.IsCurrent)
                .Include(ss => ss.Student)
                .Select(ss => new { ss.Student, ss.RollNumber, ss.SectionId });

            if (request.SortBy.Equals("name", StringComparison.OrdinalIgnoreCase))
            {
                studentSectionsQuery = studentSectionsQuery
                    .OrderBy(ss => ss.Student!.FirstName)
                    .ThenBy(ss => ss.Student!.LastName);
            }
            else
            {
                studentSectionsQuery = studentSectionsQuery
                    .OrderBy(ss => ss.RollNumber ?? 0)
                    .ThenBy(ss => ss.Student!.FirstName);
            }

            var students = await studentSectionsQuery.ToListAsync(cancellationToken);
            var studentIds = students.Select(s => s.Student!.Id).ToList();

            // Fetch saved marks for all students in this exam
            var savedMarks = await _context.StudentMarks
                .Where(sm => sm.ExamId == request.ExamId && studentIds.Contains(sm.StudentId))
                .ToListAsync(cancellationToken);

            // Group marks by student
            var marksByStudent = savedMarks
                .GroupBy(sm => sm.StudentId)
                .ToDictionary(g => g.Key, g => g.ToDictionary(sm => sm.SubjectId, sm => sm));

            return new MarksEntryFormDto
            {
                ExamId = exam.Id,
                ExamName = exam.Name,
                ClassId = examClass.ClassId,
                ClassName = examClass.Class?.Name ?? "",
                TotalStudents = students.Count,
                MarksEntryStatus = examClass.MarksEntryStatus.ToString(),
                Subjects = subjects
                    .Select(s => new SubjectForMarksDto { Id = s.SubjectId, Name = s.Name, MaxMarks = s.MaxMarks })
                    .ToList(),
                Students = students
                    .Select(s => new StudentMarksDto
                    {
                        StudentId = s.Student!.Id,
                        StudentName = $"{s.Student.FirstName} {s.Student.LastName}",
                        RollNumber = (s.RollNumber ?? 0).ToString(),
                        SectionId = s.SectionId,
                        SectionName = section.SectionName,
                        SubjectMarks = subjects
                            .ToDictionary(
                                subject => subject.SubjectId,
                                subject => 
                                {
                                    var mark = marksByStudent.ContainsKey(s.Student.Id) && 
                                              marksByStudent[s.Student.Id].ContainsKey(subject.SubjectId)
                                        ? marksByStudent[s.Student.Id][subject.SubjectId]
                                        : null;
                                    
                                    return new SubjectMarkDto
                                    {
                                        Obtained = mark?.MarksObtained,
                                        IsAbsent = mark?.IsAbsent ?? false
                                    };
                                }
                            )
                    })
                    .ToList(),
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    public class GetStudentMarksQueryHandler : IRequestHandler<GetStudentMarksQuery, StudentMarksDto>
    {
        private readonly IApplicationDbContext _context;
        public GetStudentMarksQueryHandler(IApplicationDbContext context) => _context = context;
        
        public async Task<StudentMarksDto> Handle(GetStudentMarksQuery request, CancellationToken cancellationToken)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == request.StudentId, cancellationToken);

            if (student == null)
                throw new InvalidOperationException($"Student with ID {request.StudentId} not found");

            var marks = await _context.StudentMarks
                .Where(m => m.StudentId == request.StudentId)
                .Include(m => m.Exam)
                .Include(m => m.ExamSubject)
                .GroupBy(m => m.ExamId)
                .Select(g => new
                {
                    ExamId = g.Key,
                    ExamName = g.First().Exam!.Name,
                    TotalMarks = g.Sum(m => m.MarksObtained.HasValue ? m.MarksObtained.Value : 0),
                    SubjectCount = g.Count(),
                    Marks = g.Select(m => new { m.SubjectId, m.MarksObtained }).ToList()
                })
                .ToListAsync(cancellationToken);

            return new StudentMarksDto
            {
                StudentId = student.Id,
                StudentName = $"{student.FirstName} {student.LastName}",
                RollNumber = "",
                SubjectMarks = new(),
                Total = marks.Sum(m => m.TotalMarks),
                Percentage = null,
                Grade = null
            };
        }
    }

    public class GetClassMarksQueryHandler : IRequestHandler<GetClassMarksQuery, List<StudentMarksDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetClassMarksQueryHandler(IApplicationDbContext context) => _context = context;
        
        public async Task<List<StudentMarksDto>> Handle(GetClassMarksQuery request, CancellationToken cancellationToken)
        {
            var classEntity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);
            if (classEntity == null)
                throw new InvalidOperationException($"Class with ID {request.ClassId} not found");

            var students = await _context.StudentSections
                .Where(ss => ss.SectionId == request.ClassId)
                .Include(ss => ss.Student)
                .Select(ss => new { ss.Student, ss.RollNumber })
                .ToListAsync(cancellationToken);

            var result = new List<StudentMarksDto>();

            foreach (var studentData in students)
            {
                var student = studentData.Student;
                var marks = await _context.StudentMarks
                    .Where(m => m.StudentId == student!.Id && m.ExamId == request.ExamId)
                    .Include(m => m.Exam)
                    .ToListAsync(cancellationToken);

                var totalMarks = marks
                    .Where(m => m.MarksObtained.HasValue)
                    .Sum(m => m.MarksObtained.Value);

                result.Add(new StudentMarksDto
                {
                    StudentId = student!.Id,
                    StudentName = $"{student.FirstName} {student.LastName}",
                    RollNumber = (studentData.RollNumber ?? 0).ToString()
                });
            }

            return result;
        }
    }
}
