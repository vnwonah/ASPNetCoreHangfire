using System;

namespace AspNetCoreHangfire.Models
{
    public class Todo
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime DueDate { get; set; }
        public bool Completed { get; set; }
    }
}
