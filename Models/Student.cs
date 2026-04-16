namespace StudentManagementSystem.Models
{
    public class Student
    {
        public int Id { get; set; }   // Primary Key
        public string Name { get; set; }
        public int Age { get; set; }
        public string Course { get; set; }

        public string Phone { get; set; }
    }
}
