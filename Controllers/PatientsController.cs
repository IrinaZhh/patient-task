using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientApi.Data;
using PatientApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PatientApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all patients.
        /// </summary>
        /// <returns>List of all patients</returns>
        /// <response code="200">Returns the list of patients</response>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<ActionResult<IEnumerable<Models.Patient>>> GetPatients()
        {
            return await _context.Patients.ToListAsync();
        }

        /// <summary>
        /// Retrieves a patient by their ID.
        /// </summary>
        /// <param name="id">The unique identifier of the patient</param>
        /// <returns>A patient object</returns>
        /// <response code="200">Returns the patient</response>
        /// <response code="404">Patient not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Models.Patient>> GetPatient(Guid id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();
            return patient;
        }

        /// <summary>
        /// Creates a new patient.
        /// </summary>
        /// <param name="patient">The patient data</param>
        /// <returns>Created patient</returns>
        /// <response code="201">The patient was created successfully</response>
        [HttpPost]
        [ProducesResponseType(201)]
        public async Task<ActionResult<Models.Patient>> CreatePatient(Models.Patient patient)
        {
            patient.Id = Guid.NewGuid();
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, patient);
        }

        /// <summary>
        /// Updates an existing patient.
        /// </summary>
        /// <param name="id">The unique identifier of the patient</param>
        /// <param name="patient">The updated patient data</param>
        /// <returns>Empty response</returns>
        /// <response code="204">Patient updated successfully</response>
        /// <response code="400">Invalid ID</response>
        /// <response code="404">Patient not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdatePatient(Guid id, Models.Patient patient)
        {
            if (id != patient.Id) return BadRequest();

            _context.Entry(patient).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Patients.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a patient by ID.
        /// </summary>
        /// <param name="id">The unique identifier of the patient</param>
        /// <returns>Empty response</returns>
        /// <response code="204">Patient deleted successfully</response>
        /// <response code="404">Patient not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeletePatient(Guid id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Searches patients by birthDate using FHIR-style date prefixes (e.g., gt, lt, eq).
        /// </summary>
        /// <param name="birthDate">Date with prefix (e.g., eq2024-01-01)</param>
        /// <returns>List of patients</returns>
        /// <response code="200">Returns the list of patients</response>
        /// <response code="400">Invalid date format or prefix</response>
        [HttpGet("search")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<IEnumerable<Models.Patient>>> SearchByBirthDate([FromQuery] string birthDate)
        {
            if (string.IsNullOrWhiteSpace(birthDate))
                return BadRequest("birthDate parameter is required");

            string[] prefixes = { "eq", "ne", "gt", "lt", "ge", "le" };
            string prefix = prefixes.FirstOrDefault(p => birthDate.StartsWith(p));
            string datePart = prefix != null ? birthDate.Substring(prefix.Length) : birthDate;

            if (!DateTime.TryParse(datePart, out DateTime date))
                return BadRequest("Invalid date format");

            var query = _context.Patients.AsQueryable();

            switch (prefix)
            {
                case "eq":
                case null:
                    query = query.Where(p => p.BirthDate.Date == date.Date);
                    query = query.Where(p => p.BirthDate.Date == date.Date);
                    break;
                case "ne":
                    query = query.Where(p => p.BirthDate.Date != date.Date);
                    break;
                case "gt":
                    query = query.Where(p => p.BirthDate > date);
                    break;
                case "lt":
                    query = query.Where(p => p.BirthDate < date);
                    break;
                case "ge":
                    query = query.Where(p => p.BirthDate >= date);
                    break;
                case "le":
                    query = query.Where(p => p.BirthDate <= date);
                    break;
                default:
                    return BadRequest("Invalid prefix");
            }

            return await query.ToListAsync();
        }
    }
}