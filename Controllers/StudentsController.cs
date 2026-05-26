using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace StudentManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: Add Student
        [HttpPost]
        public async Task<IActionResult> AddStudent(Student student)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetStudentById), new { id = student.Id }, student);
        }

        // GET: All Students
        [HttpGet]
        public async Task<IActionResult> GetStudents()
        {
            var students = await _context.Students.ToListAsync();
            return Ok(students);
        }

        // GET: Student by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound(new { message = $"Student with ID {id} not found" });

            return Ok(student);
        }

        // PUT: Update Student
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, Student student)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Students.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = $"Student with ID {id} not found" });

            existing.Name = student.Name;
            existing.Age = student.Age;
            existing.Course = student.Course;
            existing.Phone = student.Phone;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        // DELETE: Delete Student
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound(new { message = $"Student with ID {id} not found" });

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deleted Successfully" });
        }

        // GET: Search Students
        [HttpGet("search")]
        public async Task<IActionResult> SearchStudent(string? name, int? id)
        {
            var query = _context.Students.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(s => s.Name.Contains(name));

            if (id.HasValue)
                query = query.Where(s => s.Id == id.Value);

            var result = await query.ToListAsync();

            if (!result.Any())
                return NotFound(new { message = "No students found" });

            return Ok(result);
        }
    }
}