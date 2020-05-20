using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.DAL;
using System.Data.SqlClient;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/students")]
    
    public class StudentsController : ControllerBase
    {

        private readonly IDbService _dbService;

        public StudentsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet]
        public IActionResult GetStudents()
        {
            List<Student> _students = new List<Student>();

            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s16985;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "select st.FirstName, st.Lastname, st.BirthDate, sd.Name, e.Semester from Student st join Enrollment e on st.IdEnrollment=e.IdEnrollment join Studies sd on e.IdStudy=sd.IdStudy;";

                

                con.Open();
                var dr = com.ExecuteReader();
                while (dr.Read())
                {
                    var st = new Student();
                    st.FirstName = dr["FirstName"].ToString();
                    st.LastName = dr["LastName"].ToString();
                    st.BirthDate = DateTime.Parse(dr["BirthDate"].ToString());
                    st.StudiesName = dr["Name"].ToString();
                    st.Semester = int.Parse(dr["Semester"].ToString());

                    _students.Add(st);
                }
            }

            return Ok(_students);
        }
        public string GetStudent()
        {
            return "Kowalski, Malewski, Andrzejewski";
        }

        [HttpGet("{indexNumber}")]
        public IActionResult GetStudent(string indexNumber) // w celu usunięcia tabeli Student przez atak SQLInjection należy podać jako indexNumber ';%20DROP%20TABLE%20Student;--
        {
            var enrollment = new Enrollment();

            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s16985;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.Connection = con;             
                com.CommandText = "select e.* from Student st join Enrollment e on st.IdEnrollment=e.IdEnrollment join Studies sd on e.IdStudy=sd.IdStudy where st.indexNumber=@indexNumber;";
                com.Parameters.AddWithValue("indexNumber", indexNumber);

                con.Open();
                var dr = com.ExecuteReader();
                while (dr.Read())
                {
                    enrollment.IdEnrollment = int.Parse(dr["IdEnrollment"].ToString());
                    enrollment.Semester = int.Parse(dr["Semester"].ToString());
                    enrollment.IdStudy = int.Parse(dr["IdStudy"].ToString());
                    enrollment.StartDate = DateTime.Parse(dr["StartDate"].ToString());
                }
            }

            return Ok(enrollment);
        }


        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
     //       student.IndexNumber = $"s{new Random().Next(1, 20000)}";
            return Ok(student);
        }

        [HttpPut]
        public IActionResult UpDateStudent(int id)
        {

            return Ok("Aktualizacja dokończona");
        }

        [HttpDelete]
        public IActionResult DeleteStudent(int id)
        {
            return Ok("Usuwanie ukończone");
        }
    }
}