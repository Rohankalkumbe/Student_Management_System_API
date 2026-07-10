using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StudentsController(ApplicationDbContext context) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Student>> AddStudent(Student student)
    {
        context.Students.Add(student);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStudentById), new { id = student.Id }, student);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
    {
        var students = await context.Students.AsNoTracking().ToListAsync();
        return Ok(students);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Student>> GetStudentById(int id)
    {
        var student = await context.Students.FindAsync(id);
        return student is null
            ? NotFound(new { message = $"Student with ID {id} not found" })
            : Ok(student);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Student>> UpdateStudent(int id, Student student)
    {
        var existingStudent = await context.Students.FindAsync(id);
        if (existingStudent is null)
        {
            return NotFound(new { message = $"Student with ID {id} not found" });
        }

        existingStudent.Name = student.Name;
        existingStudent.Age = student.Age;
        existingStudent.Course = student.Course;
        existingStudent.Phone = student.Phone;

        await context.SaveChangesAsync();
        return Ok(existingStudent);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        var student = await context.Students.FindAsync(id);
        if (student is null)
        {
            return NotFound(new { message = $"Student with ID {id} not found" });
        }

        context.Students.Remove(student);
        await context.SaveChangesAsync();

        return Ok(new { message = "Deleted Successfully" });
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Student>>> SearchStudent(string? name, int? id)
    {
        var query = context.Students.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(student => student.Name.Contains(name));
        }

        if (id.HasValue)
        {
            query = query.Where(student => student.Id == id.Value);
        }

        var students = await query.ToListAsync();
        return students.Count == 0
            ? NotFound(new { message = "No students found" })
            : Ok(students);
    }
}
