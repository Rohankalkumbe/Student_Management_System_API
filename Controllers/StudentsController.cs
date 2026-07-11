using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StudentsController(IMongoDatabase database) : ControllerBase
{
    private readonly IMongoCollection<Student> _students = database.GetCollection<Student>("students");
    private readonly IMongoCollection<Counter> _counters = database.GetCollection<Counter>("counters");

    [HttpPost]
    public async Task<ActionResult<Student>> AddStudent(Student student)
    {
        student.Id = await GetNextStudentId();
        await _students.InsertOneAsync(student);

        return CreatedAtAction(nameof(GetStudentById), new { id = student.Id }, student);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
    {
        var students = await _students.Find(Builders<Student>.Filter.Empty).ToListAsync();
        return Ok(students);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Student>> GetStudentById(int id)
    {
        var student = await _students.Find(student => student.Id == id).FirstOrDefaultAsync();
        return student is null
            ? NotFound(new { message = $"Student with ID {id} not found" })
            : Ok(student);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Student>> UpdateStudent(int id, Student student)
    {
        var existingStudent = await _students.Find(existing => existing.Id == id).FirstOrDefaultAsync();
        if (existingStudent is null)
        {
            return NotFound(new { message = $"Student with ID {id} not found" });
        }

        existingStudent.Name = student.Name;
        existingStudent.Age = student.Age;
        existingStudent.Course = student.Course;
        existingStudent.Phone = student.Phone;

        await _students.ReplaceOneAsync(existing => existing.Id == id, existingStudent);
        return Ok(existingStudent);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        var student = await _students.Find(student => student.Id == id).FirstOrDefaultAsync();
        if (student is null)
        {
            return NotFound(new { message = $"Student with ID {id} not found" });
        }

        await _students.DeleteOneAsync(existing => existing.Id == id);

        return Ok(new { message = "Deleted Successfully" });
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Student>>> SearchStudent(string? name, int? id)
    {
        var filters = new List<FilterDefinition<Student>>();

        if (!string.IsNullOrWhiteSpace(name))
        {
            filters.Add(Builders<Student>.Filter.Regex(
                student => student.Name,
                new MongoDB.Bson.BsonRegularExpression(name, "i")));
        }

        if (id.HasValue)
        {
            filters.Add(Builders<Student>.Filter.Eq(student => student.Id, id.Value));
        }

        var filter = filters.Count == 0
            ? Builders<Student>.Filter.Empty
            : Builders<Student>.Filter.And(filters);
        var students = await _students.Find(filter).ToListAsync();
        return students.Count == 0
            ? NotFound(new { message = "No students found" })
            : Ok(students);
    }

    private async Task<int> GetNextStudentId()
    {
        var options = new FindOneAndUpdateOptions<Counter>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };
        var counter = await _counters.FindOneAndUpdateAsync(
            item => item.Id == "students",
            Builders<Counter>.Update.Inc(item => item.Value, 1),
            options);

        return counter.Value;
    }

    private sealed class Counter
    {
        [MongoDB.Bson.Serialization.Attributes.BsonId]
        public string Id { get; set; } = string.Empty;

        public int Value { get; set; }
    }
}
