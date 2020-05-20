using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.DTOs.Requests;
using WebApplication1.DTOs.Responses;

namespace WebApplication1.Services
{
	public class SqlServerDbService : IStudentsDbService
	{
		public EnrollStudentResponse EnrollStudent(EnrollStudentRequest request)
		{
            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s16985;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                var response = new EnrollStudentResponse();
                var semester = 1;

                if (con.State == ConnectionState.Closed)
                    con.Open();

                var trans = con.BeginTransaction();

                com.Connection = con;
                com.Transaction = trans;

                try
                {
                    com.CommandText = "SELECT IdStudy FROM Studies WHERE Name=@Name";
                    com.Parameters.AddWithValue("Name", request.Studies);

                    var dr = com.ExecuteReader();

                    if (!dr.Read())
                    {
                        dr.Close();
                        trans.Rollback();
                        response.Message = "Studia nie istnieją!";
                        return response;
                    }

                    var idStudy = (int)dr["IdStudy"];
                    dr.Close();

                    com.Parameters.Clear();
                    com.CommandText = "SELECT IdEnrollment FROM Enrollment WHERE IdStudy=@IdStudy AND Semester=@Semester";
                    com.Parameters.AddWithValue("IdStudy", idStudy);
                    com.Parameters.AddWithValue("Semester", semester);

                    int idEnrollment;

                    dr = com.ExecuteReader();
                    if (!dr.Read())
                    {
                        dr.Close();

                        com.Parameters.Clear();
                        com.CommandText = "SELECT MAX(IdEnrollment) FROM Enrollment";

                        idEnrollment = (int)com.ExecuteScalar() + 1;

                        com.CommandText = "INSERT INTO Enrollment(IdEnrollment, Semester, IdStudy, StartDate)" +
                            " VALUES (@IdEnrollment, @Semester, @IdStudy, @StartDate)";
                        com.Parameters.AddWithValue("IdEnrollment", idEnrollment);
                        com.Parameters.AddWithValue("Semester", semester);
                        com.Parameters.AddWithValue("IdStudy", idStudy);
                        com.Parameters.AddWithValue("StartDate", DateTime.Today);
                        com.CommandType = CommandType.Text;
                        com.ExecuteNonQuery();
                    }
                    else
                    {
                        idEnrollment = (int)dr["IdEnrollment"];
                        dr.Close();
                    }

                    com.Parameters.Clear();
                    com.CommandText = "SELECT IndexNumber FROM Student";

                    dr = com.ExecuteReader();
                    string indexNumber;
                    while (dr.Read())
                    {
                        indexNumber = (string)dr["IndexNumber"];
                        if (indexNumber == request.IndexNumber)
                        {
                            response.Message = "Student o podanym numerze indeksu już istnieje w bazie!";
                            return response;
                        }
                    }
                    dr.Close();

                    com.Parameters.Clear();
                    com.CommandText = "INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment)" +
                            " VALUES (@IndexNumber, @FirstName, @LastName, @BirthDate, @IdEnrollment)";
                    com.Parameters.AddWithValue("IndexNumber", request.IndexNumber);
                    com.Parameters.AddWithValue("FirstName", request.FirstName);
                    com.Parameters.AddWithValue("LastName", request.LastName);
                    com.Parameters.AddWithValue("BirthDate", request.BirthDate);
                    com.Parameters.AddWithValue("IdEnrollment", idEnrollment);
                    com.CommandType = CommandType.Text;
                    com.ExecuteNonQuery();

                }
                catch (SqlException e)
                {
                    trans.Rollback();
                    response.Message = e.Message;
                    return response;
                }

                trans.Commit();

                response.IndexNumber = request.IndexNumber;
                response.Semester = semester;
                response.Message = "Zapisano studenta na semestr";
                return response;
            }
        }

		public PromoteStudentsResponse PromoteStudents(PromoteStudentsRequest request)
		{
            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s16985;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                var response = new PromoteStudentsResponse();
                Object idEnrollment;

                if (con.State == ConnectionState.Closed)
                    con.Open();

                var trans = con.BeginTransaction();

                com.Connection = con;
                com.Transaction = trans;

                try
                {
                    com.Parameters.Clear();
                    com.CommandType = CommandType.StoredProcedure;
                    com.CommandText = "PromoteStudents";
                    com.Parameters.AddWithValue("Studies", request.Studies);
                    com.Parameters.AddWithValue("Semester", request.Semester);

                    var returnValue = com.Parameters.Add("@ReturnValue", SqlDbType.Int);
                    returnValue.Direction = ParameterDirection.ReturnValue;

                    com.ExecuteNonQuery();

                    if ((int)returnValue.Value == 0)
                    {
                        response.Message = "Podany semestr nie istnieje!";
                        return response;
                    }
                    else
                    {
                        idEnrollment = returnValue.Value;
                    }
                }
                catch (SqlException e)
                {
                    trans.Rollback();
                    response.Message = e.Message;
                    return response;
                }

                trans.Commit();

                response.IdEnrollment = (int)idEnrollment;
                response.Message = "Ok";
                return response;
            }
        }

        public bool CheckIndex(string indexNumber)
        {
            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s16985;Integrated Security=True"))
            using (var com = new SqlCommand())
            {

                if (con.State == ConnectionState.Closed)
                    con.Open();

                var trans = con.BeginTransaction();

                com.Connection = con;
                com.Transaction = trans;

                try
                {
                    com.CommandText = "SELECT IndexNumber FROM Student WHERE IndexNumber=@IndexNumber";
                    com.Parameters.AddWithValue("IndexNumber", indexNumber);
                    com.CommandType = CommandType.Text;

                    var existingIndexNumber = (string)com.ExecuteScalar();

                    if (existingIndexNumber == null)
                    {
                        return false;
                    }
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e);
                }

                return true;
            }
        }
    }
}
