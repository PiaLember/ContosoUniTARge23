using ContosoUniTARge23.Data;
using ContosoUniTARge23.Models;
using ContosoUniTARge23.Models.SchoolViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniTARge23.Controllers
{
    public class InstructorsController : Controller
    {
        private readonly SchoolContext _context;

        public InstructorsController(SchoolContext context)
        {
            _context = context;
        }

        // GET: Instructors
        public async Task<IActionResult> Index(int? id, int? courseId)
        {
            var viewModel = new InstructorIndexData();
            viewModel.Instructors = await _context.Instructors
                  .Include(i => i.OfficeAssignment)
                  .Include(c => c.CourseAssignments)
                    .ThenInclude(x => x.Course)
                        .ThenInclude(y => y.Enrollments)
                            .ThenInclude(z => z.Student)
                  .Include(a => a.CourseAssignments)
                    .ThenInclude(b => b.Course)
                        .ThenInclude(d => d.Department)
                  .AsNoTracking()
                  .OrderBy(e => e.LastName)
                  .ToListAsync();

            if (id != null)
            {
                ViewData["InstructorId"] = id.Value;
                Instructor instructor = viewModel.Instructors.Where(
                    i => i.Id == id.Value).Single();
                viewModel.Courses = instructor.CourseAssignments.Select(s => s.Course);
            }

            if (courseId != null)
            {
                ViewData["CourseId"] = courseId.Value;

                var selectedCourse = viewModel.Courses.Where(x => x.CourseId == courseId).Single();
                await _context.Entry(selectedCourse).Collection(x => x.Enrollments).LoadAsync();
                foreach (Enrollment enrollment in selectedCourse.Enrollments)
                {
                    await _context.Entry(enrollment).Reference(x => x.Student).LoadAsync();
                }
                viewModel.Enrollments = selectedCourse.Enrollments;

            }

            return View(viewModel);
        }


    }
}
