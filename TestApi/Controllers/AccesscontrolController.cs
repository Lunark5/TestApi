using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using Npgsql;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using TestApi.Models;

namespace TestApi.Controllers
{
    public class AccesscontrolController : Controller
    {
        private readonly IConfiguration _configuration;
        private IWebHostEnvironment _appEnvironment;
        public AccesscontrolController(IConfiguration configuration, IWebHostEnvironment appEnvironment)
        {
            _configuration = configuration;
            _appEnvironment = appEnvironment;
        }
        public ActionResult Index()
        {
            string query = @"select UserId as ""UserId""
            from Users
            ";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("UsersAppCon");
            NpgsqlDataReader myReader;
            List<string> ids = new List<string>();
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        ids.Add(myReader["UserId"].ToString());
                    }

                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();

                }
            }
            return View("Index", ids.Count);
        }
        [HttpGet]
        public ActionResult Users(int? id,string name = "")
        {
            List<User> userBase = new List<User>();
            if (id == null)
            {
                string query;
                query = @"
                        select UserId as ""UserId"",
                        UserName as ""UserName"",
                        UserSurname as ""UserSurname"",
                        UserPatronymic as ""UserPatronymic"",
                        PhotoFileName as ""PhotoFileName"",
                        IdentifiersName as ""IdentifiersName""
                        from Users
                        ";
                if (name != "")
                {
                    query = @"
                        select UserId as ""UserId"",
                        UserName as ""UserName"",
                        UserSurname as ""UserSurname"",
                        UserPatronymic as ""UserPatronymic"",
                        PhotoFileName as ""PhotoFileName"",
                        IdentifiersName as ""IdentifiersName""
                        from Users
                        where username = @UserName
                        ";
                }

                DataTable table = new DataTable();
                string sqlDataSource = _configuration.GetConnectionString("UsersAppCon");
                NpgsqlDataReader myReader;
                using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
                {
                    myCon.Open();
                    using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                    {
                        if (name != "")
                        {
                            myCommand.Parameters.AddWithValue("@UserName", name);
                        }
                        myReader = myCommand.ExecuteReader();
                        while (myReader.Read())
                        {
                            User newUser = new User();
                            newUser.Id = Convert.ToInt32(myReader["UserId"]);
                            newUser.Name = myReader["UserName"].ToString();
                            newUser.Surname = myReader["UserSurname"].ToString();
                            newUser.Patronymic = myReader["UserPatronymic"].ToString();
                            newUser.Photo = myReader["PhotoFileName"].ToString();
                            newUser.Identifiers = myReader["IdentifiersName"].ToString();
                            userBase.Add(newUser);
                        }

                        table.Load(myReader);
                        myReader.Close();
                        myCon.Close();

                    }
                }
                return View(userBase);
            }
            else
            {
                string query = @"
                        select * from Users 
                        where userid = @UserId
                        ";
                DataTable table = new DataTable();
                string sqlDataSource = _configuration.GetConnectionString("UsersAppCon");
                NpgsqlDataReader myReader;
                User newUser = new User();
                using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
                {
                    myCon.Open();
                    using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                    {

                        myCommand.Parameters.AddWithValue("@UserId", id);
                        myReader = myCommand.ExecuteReader();
                        while (myReader.Read())
                        {
                            newUser.Id = Convert.ToInt32(myReader["UserId"]);
                            newUser.Name = myReader["UserName"].ToString();
                            newUser.Surname = myReader["UserSurname"].ToString();
                            newUser.Patronymic = myReader["UserPatronymic"].ToString();
                            newUser.Photo = myReader["PhotoFileName"].ToString();
                            newUser.Identifiers = myReader["IdentifiersName"].ToString();
                            userBase.Add(newUser);
                        }
                        table.Load(myReader);
                        myReader.Close();
                        myCon.Close();

                    }
                }
                return View("User", newUser);
            }
            
        }
        [HttpPost]
        public ActionResult DeleteUser(int id)
        {
            string path = _appEnvironment.ContentRootPath + "/Content/";
            string photoPath = "";
            string identifiersPath = "";

            string query = @"
                delete from Users
                where UserId = @UserId
                ";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("UsersAppCon");
            NpgsqlDataReader myReader;
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@UserId", id);
                    myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        photoPath = myReader["PhotoFileName"].ToString();
                        identifiersPath = myReader["IdentifiersName"].ToString();
                    }
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();

                }
            }
            if (System.IO.File.Exists(Path.Combine(path, photoPath)))
            {
                System.IO.File.Delete(Path.Combine(path, photoPath));
            }
            if (System.IO.File.Exists(Path.Combine(path, identifiersPath)))
            {
                System.IO.File.Delete(Path.Combine(path, identifiersPath));
            }
            return View("Get", "Пользователь успешно удален");

        }
        [HttpGet]
        public ActionResult AddUser()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> AddUser(IFormFile photo, IFormFile identifiers, string name, string surname, string patronymic)
        {
            string saveName = "";
            saveName += name;
            string saveSurname = "";
            saveSurname += surname;
            string savePatronymic = "";
            savePatronymic += patronymic;
            string savePhoto = "";
            string saveIdentifiers = "";
            string path = _appEnvironment.ContentRootPath + "/Content/";
            if (photo != null)
            {

                savePhoto = String.Format(@"{0}.jpg", Guid.NewGuid()); 
                using (FileStream fs = new FileStream(Path.Combine(path, savePhoto), FileMode.Create))
                {
                    await photo.CopyToAsync(fs);
                }
            }
            if (identifiers != null)
            {
                saveIdentifiers = String.Format(@"{0}.jpg", Guid.NewGuid());
                using (FileStream fs = new FileStream(Path.Combine(path, saveIdentifiers), FileMode.Create))
                {
                    await identifiers.CopyToAsync(fs);
                }
            }
            
            
            string query = @"
            insert into Users(UserName, UserSurname,UserPatronymic, PhotoFileName, IdentifiersName)
            values (@UserName, @UserSurname, @UserPatronymic, @PhotoFileName, @IdentifiersName)
            ";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("UsersAppCon");
            NpgsqlDataReader myReader;
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@UserName", saveName);
                    myCommand.Parameters.AddWithValue("@UserSurname", saveSurname);
                    myCommand.Parameters.AddWithValue("@UserPatronymic",savePatronymic);
                    myCommand.Parameters.AddWithValue("@PhotoFileName", savePhoto);
                    myCommand.Parameters.AddWithValue("@IdentifiersName", saveIdentifiers);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();

                }
            }
            return View("Get", "Пользователь успешно добавлен");
        }
        [HttpGet]
        public ActionResult UsersPut(int id)
        {
            return View("EditUser", id);
        }
        [HttpPost]
        public async Task<ActionResult> UsersPut(int id, IFormFile photo, IFormFile identifiers, string name, string surname, string patronymic)
        {
            string saveName = "";
            saveName += name;
            string saveSurname = "";
            saveSurname += surname;
            string savePatronymic = "";
            savePatronymic += patronymic;
            string savePhoto = "";
            string saveIdentifiers = "";
            string path = _appEnvironment.ContentRootPath + "/Content/";
            if (photo != null)
            {

                savePhoto = String.Format(@"{0}.jpg", Guid.NewGuid());
                using (FileStream fs = new FileStream(Path.Combine(path, savePhoto), FileMode.Create))
                {
                    await photo.CopyToAsync(fs);
                }
            }
            if (identifiers != null)
            {
                saveIdentifiers = String.Format(@"{0}.jpg", Guid.NewGuid());
                using (FileStream fs = new FileStream(Path.Combine(path, saveIdentifiers), FileMode.Create))
                {
                    await identifiers.CopyToAsync(fs);
                }
            }

            string query = @"
            update Users
            set UserName = @UserName,
            UserSurname = @UserSurname,
            UserPatronymic = @UserPatronymic,
            PhotoFileName = @PhotoFileName,
            IdentifiersName = @IdentifiersName
            where UserId = @UserId
            ";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("UsersAppCon");
            NpgsqlDataReader myReader;
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@UserId", id);
                    myCommand.Parameters.AddWithValue("@UserName", saveName);
                    myCommand.Parameters.AddWithValue("@UserSurname", saveSurname);
                    myCommand.Parameters.AddWithValue("@UserPatronymic", savePatronymic);
                    myCommand.Parameters.AddWithValue("@PhotoFileName", savePhoto);
                    myCommand.Parameters.AddWithValue("@IdentifiersName", saveIdentifiers);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();

                }
            }
            return View("Get", "Пользователь изменен");
        }

    }

}