#!/usr/bin/env dotnet-script
#r "nuget: Npgsql, 8.0.1"

using Npgsql;
using System;

var connString = "Host=localhost;Port=5432;Database=sms_db;Username=postgres;Password=Pass@123";

try
{
    using var conn = new NpgsqlConnection(connString);
    conn.Open();
    
    // Check total records
    using var cmd1 = new NpgsqlCommand("SELECT COUNT(*) FROM student_attendances", conn);
    var total = Convert.ToInt64(cmd1.ExecuteScalar());
    Console.WriteLine($"Total attendance records: {total}");
    
    if (total > 0)
    {
        // Check records with empty/invalid section_id
        using var cmd2 = new NpgsqlCommand("SELECT COUNT(*) FROM student_attendances WHERE section_id = '00000000-0000-0000-0000-000000000000' OR section_id IS NULL", conn);
        var invalid = Convert.ToInt64(cmd2.ExecuteScalar());
        Console.WriteLine($"Records with invalid section_id: {invalid}");
        
        // Show sample of problematic data
        using var cmd3 = new NpgsqlCommand("SELECT id, student_id, section_id, attendance_date FROM student_attendances LIMIT 5", conn);
        using var reader = cmd3.ExecuteReader();
        Console.WriteLine("\nSample records:");
        while (reader.Read())
        {
            Console.WriteLine($"  ID: {reader["id"]}, StudentID: {reader["student_id"]}, SectionID: {reader["section_id"]}, Date: {reader["attendance_date"]}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
