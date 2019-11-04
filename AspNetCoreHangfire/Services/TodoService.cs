using AspNetCoreHangfire.Data;
using AspNetCoreHangfire.Models;
using Hangfire;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AspNetCoreHangfire.Services
{
    public class TodoService
    {
        private AppDbContext _context;
        public TodoService(AppDbContext context)
        {
            _context = context;
        }
        public void ProcessUploadedFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException();
            //we are throwing an argument if an invalid filepath is given to us to process

            StreamReader file = null;
            try
            {
                string line;
                file = new StreamReader(filePath);
                while ((line = file.ReadLine()) != null)
                {
                    //we are getting eac part of the todo into an array. 
                    //parts[0] will be the Todo Text and parts[1] will be the Due date
                    var parts = line.Split(",");

                    //first part of this if statement checks that the Todo Text is a valid non empty text
                    //second part checks that we are passing in a valid date time
                    if(!string.IsNullOrWhiteSpace(parts?[0]) && DateTime.TryParse(parts?[1], out DateTime date))
                    {
                        BackgroundJob.Enqueue(() => SaveNewTodoItem(parts[0], date));
                    }
                    
                }

                file.Close();
            }
            catch (Exception e)
            {

            }
            finally
            {
                if (file is object)
                    file.Close();

                //delete the file
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        public async Task SaveNewTodoItem(string text, DateTime dueDate)
        {
            var todo = new Todo
            {
                Text = text,
                DueDate = dueDate
            };

            await _context.AddAsync(todo);
            await _context.SaveChangesAsync();

        }


    }

    
}
