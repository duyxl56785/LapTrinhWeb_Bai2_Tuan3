using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Models;

namespace StudentManagement.Controllers
{
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Students
        public async Task<IActionResult> Index(string searchString)
        {
            var students = from s in _context.Students select s;

            if (!string.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.FullName.ToLower().Contains(searchString.ToLower())
                                            || s.StudentCode.ToLower().Contains(searchString.ToLower()));
            }

            return View(await students.OrderByDescending(s => s.Id).ToListAsync());
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StudentCode,FullName,Email,DateOfBirth,Major,Gpa")] Student student)
        {
            // Kiểm tra trùng mã sinh viên
            if (await _context.Students.AnyAsync(s => s.StudentCode == student.StudentCode))
            {
                ModelState.AddModelError("StudentCode", "Mã sinh viên này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                // Chuyển DateTime sang UTC để tương thích với PostgreSQL timestamptz nếu cần
                student.DateOfBirth = DateTime.SpecifyKind(student.DateOfBirth, DateTimeKind.Utc);
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StudentCode,FullName,Email,DateOfBirth,Major,Gpa")] Student student)
        {
            if (id != student.Id) return NotFound();

            if (await _context.Students.AnyAsync(s => s.StudentCode == student.StudentCode && s.Id != student.Id))
            {
                ModelState.AddModelError("StudentCode", "Mã sinh viên này đã được sử dụng bởi người khác.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    student.DateOfBirth = DateTime.SpecifyKind(student.DateOfBirth, DateTimeKind.Utc);
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}