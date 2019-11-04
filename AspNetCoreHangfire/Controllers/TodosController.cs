using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AspNetCoreHangfire.Data;
using AspNetCoreHangfire.Models;
using Hangfire;
using Microsoft.AspNetCore.Http;
using System.IO;
using AspNetCoreHangfire.Services;

namespace AspNetCoreHangfire.Controllers
{
    public class TodosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly TodoService _todoService;

        public TodosController(
            AppDbContext context,
            TodoService todoService)
        {
            _context = context;
            _todoService = todoService;
        }

        // GET: Todos
        public async Task<IActionResult> Index()
        {
            if(TempData["success"] is object)
            {
                ViewBag.Success = TempData["success"];
            }
            else if(TempData["error"] is object)
            {
                ViewBag.Error = TempData["error"];
            }

            TempData.Clear();
            return View(await _context.Todo.ToListAsync());
        }

        // GET: Todos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var todo = await _context.Todo
                .FirstOrDefaultAsync(m => m.Id == id);
            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
         {
            try
            {
                if (file == null || file.Length == 0)
                {
                    TempData["error"] = "File not Selected";
                    return RedirectToAction("Index");
                }
                    

                var path = Path.Combine(
                            Directory.GetCurrentDirectory(), "wwwroot",
                            file.FileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                BackgroundJob.Enqueue(() => _todoService.ProcessUploadedFile(path));
                TempData["success"] = "The Todo Items are being created";

            }
            catch (Exception)
            {
                TempData["error"] = "An Error occured, please retry";
            }
           
            
            return RedirectToAction("Index");
        }

        // GET: Todos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Todos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Text,DueDate,Completed")] Todo todo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(todo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(todo);
        }

        // GET: Todos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var todo = await _context.Todo.FindAsync(id);
            if (todo == null)
            {
                return NotFound();
            }
            return View(todo);
        }

        // POST: Todos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Text,DueDate,Completed")] Todo todo)
        {
            if (id != todo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(todo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TodoExists(todo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(todo);
        }

        // GET: Todos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var todo = await _context.Todo
                .FirstOrDefaultAsync(m => m.Id == id);
            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        // POST: Todos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var todo = await _context.Todo.FindAsync(id);
            _context.Todo.Remove(todo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TodoExists(int id)
        {
            return _context.Todo.Any(e => e.Id == id);
        }

        private async Task AddTodoWithBackgroundJob(string text, DateTime dueDate)
        {
            var todo = new Todo
            {
                Text = text,
                DueDate = dueDate,
            };

            await _context.AddAsync(todo);
            await _context.SaveChangesAsync();
        }
    }
}
