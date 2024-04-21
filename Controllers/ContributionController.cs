using API.Data;
using API.Dtos;
using API.EmailService;
using API.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContributionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        public ContributionController(AppDbContext context, UserManager<User> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        // GET: api/contributions
        [HttpGet]
        [Authorize(Roles = "MarketingManager")]
        public async Task<ActionResult<IEnumerable<ContributionDto>>> GetContributions()
        {
            var contributions = await _context.Contributions
                .Include(c => c.Documents) 
                .Select(c => new ContributionDto
                {
                    
                    ContributionId = c.ContributionID,
                    Title = c.Title,
                    SubmissionDate = c.SubmissionDate,
                    Content = c.Content,
                    SelectedForPublication = c.SelectedForPublication,
                    Commented = c.Commented,
                    Likes = c.Likes,
                    Dislikes = c.Dislikes,
                    Views = c.Views,
                    Status = c.Status,
                    EventID = c.EventID,
                    UserID = c.UserID,
                    UploadedDocuments = c.Documents.Select(d => new UploadedDocumentDto
                    {
                        Id = d.Id,
                        Content = d.Content,
                        ContentType = d.ContentType
                    }).ToList() 
                }).ToListAsync(); 

            return contributions;
        }

        // GET: api/contributions/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ContributionDto>> GetContribution(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }
            var contribution = await _context.Contributions.FindAsync(id);
            if (contribution == null)
            {
                return NotFound();
            }
            var contributionDTO = new ContributionDto
            {
                ContributionId = contribution.ContributionID,
                Title = contribution.Title,
                SubmissionDate = contribution.SubmissionDate,
                Content = contribution.Content,
                SelectedForPublication = contribution.SelectedForPublication,
                Commented = contribution.Commented,
                Likes = contribution.Likes,
                Dislikes = contribution.Dislikes,
                isLike = false,
                isDislike = false,
                Views = contribution.Views,
                Status = contribution.Status,
                EventID = contribution.EventID,
            };
            var existingLike = await _context.LikeDislikes.FirstOrDefaultAsync(l => l.ContributionId == contributionDTO.ContributionId && l.UserId == currentUser.Id);
            if (existingLike != null)
            {
                if (existingLike.Like)
                {
                    contributionDTO.isLike = true;
                    contributionDTO.isDislike = false;
                }
                else if (existingLike.Dislike)
                {
                    contributionDTO.isLike = false;
                    contributionDTO.isDislike = true;
                }
            }
            return contributionDTO;
        }
        // GET: api/contributions/user
        [HttpGet("user")]
        [Authorize(Roles = " Student")]
        public async Task<ActionResult<IEnumerable<ContributionDto>>> GetContributionsByUser()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }
            var contributions = await _context.Contributions
                .Include(c => c.Documents) 
                .Where(c => c.UserID == currentUser.Id)
                .Select(c => new ContributionDto
                {
                    ContributionId = c.ContributionID,
                    Title = c.Title,
                    SubmissionDate = c.SubmissionDate,
                    Content = c.Content,
                    SelectedForPublication = c.SelectedForPublication,
                    Commented = c.Commented,
                    Likes = c.Likes,
                    Dislikes = c.Dislikes,
                    Views = c.Views,
                    Status = c.Status,
                    EventID = c.EventID,
                    UploadedDocuments = c.Documents.Select(d => new UploadedDocumentDto
                    {
                        Id = d.Id,
                        Content = d.Content,
                        ContentType = d.ContentType
                    }).ToList() 
                }).ToListAsync(); 
            return contributions;
        }
        //GET : api/contributions/faculty
        [HttpGet("faculty")]
        [Authorize(Roles = "MarketingCoordinator , Student" )]
        public async Task<ActionResult<IEnumerable<ContributionDto>>> GetContributionsByFaculty()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }
            
            var contributions = await _context.Contributions
                .Where(c => c.FacultyID == currentUser.FacultyID)
                .Select(c => new ContributionDto
                {

                    ContributionId = c.ContributionID,
                    Title = c.Title,
                    SubmissionDate = c.SubmissionDate,
                    Content = c.Content,
                    SelectedForPublication = c.SelectedForPublication,
                    Commented = c.Commented,
                    Likes = c.Likes,
                    Dislikes = c.Dislikes,
                    isLike = false,
                    isDislike = false,
                    Views = c.Views,
                    Status = c.Status,
                    EventID = c.EventID,
                })
                .ToListAsync();
            foreach( var contribution in contributions)
            {
                var existingLike = await _context.LikeDislikes.FirstOrDefaultAsync(l => l.ContributionId == contribution.ContributionId && l.UserId == currentUser.Id);
                if(existingLike != null)
                {
                    if (existingLike.Like)
                    {
                        contribution.isLike = true;
                        contribution.isDislike = false;
                    }else if (existingLike.Dislike)
                    {
                        contribution.isLike = false;
                        contribution.isDislike = true;
                    }
                }
            }
            foreach (var contribution in contributions)
            {
                var uploadedDocuments = await _context.UploadedDocuments
                    .Where(d => d.ContributionId == contribution.ContributionId)
                    .Select(d => new UploadedDocumentDto
                    {
                        Id = d.Id,
                        Content = d.Content,
                        ContentType = d.ContentType
                    })
                    .ToListAsync();

                // Gán danh sách các tệp đính kèm vào contributionDTO
                contribution.UploadedDocuments = uploadedDocuments;
            }
            return contributions;
        }
        //GET : api/contributions/event
        [HttpGet("event")]
        public async Task<ActionResult<IEnumerable<ContributionDto>>> GetContributionsByEvent(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var contributions = await _context.Contributions
                .Where(c => c.EventID == id)
                .Select(c => new ContributionDto
                {

                    ContributionId = c.ContributionID,
                    Title = c.Title,
                    SubmissionDate = c.SubmissionDate,
                    Content = c.Content,
                    SelectedForPublication = c.SelectedForPublication,
                    Commented = c.Commented,
                    Likes = c.Likes,
                    Dislikes = c.Dislikes,
                    isLike = false,
                    isDislike = false,
                    Views = c.Views,
                    Status = c.Status,
                    EventID = c.EventID,
                })
                .ToListAsync();
            foreach (var contribution in contributions)
            {
                var existingLike = await _context.LikeDislikes.FirstOrDefaultAsync(l => l.ContributionId == contribution.ContributionId && l.UserId == currentUser.Id);
                if (existingLike != null)
                {
                    if (existingLike.Like)
                    {
                        contribution.isLike = true;
                        contribution.isDislike = false;
                    }
                    else if (existingLike.Dislike)
                    {
                        contribution.isLike = false;
                        contribution.isDislike = true;
                    }
                }
            }
            foreach (var contribution in contributions)
            {
                var uploadedDocuments = await _context.UploadedDocuments
                    .Where(d => d.ContributionId == contribution.ContributionId)
                    .Select(d => new UploadedDocumentDto
                    {
                        Id = d.Id,
                        Content = d.Content,
                        ContentType = d.ContentType
                    })
                    .ToListAsync();

                // Gán danh sách các tệp đính kèm vào contributionDTO
                contribution.UploadedDocuments = uploadedDocuments;
            }
            return contributions;
        }
        // GET: api/contributions/{id}/file
        [HttpGet("{id}/file")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var uploadedDocument = await _context.UploadedDocuments.FirstOrDefaultAsync(d => d.ContributionId == id);
            if (uploadedDocument == null)
            {
                return NotFound();
            }
            string fileExtension = ".dat"; 
            switch (uploadedDocument.ContentType)
            {
                case "application/msword":
                    fileExtension = ".doc";
                    break;
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                    fileExtension = ".docx";
                    break;
                case "image/jpeg":
                    fileExtension = ".jpg";
                    break;
                case "image/png":
                    fileExtension = ".png";
                    break;
                default:
                    break;
            }

            string fileName = $"contribution_{id}{fileExtension}";
            var tempFilePath = Path.Combine(Path.GetTempPath(), fileName);
            System.IO.File.WriteAllBytes(tempFilePath, uploadedDocument.Content);
            return PhysicalFile(tempFilePath, "application/octet-stream", fileName);
        }

        // GET: api/contributions/{id}/comment
        [HttpGet("{id}/comment")]
        public async Task<ActionResult<IEnumerable<CommentDTO>>> GetComment(int id)
        {
            var comments = await _context.Comments
               .Where(c => c.ContributionId == id)
               .Select(c => new CommentDTO
               {
                   Id = c.Id,
                   ContributionId = c.ContributionId,
                   UserId = c.UserId,
                   Content = c.Content,
                   Date = c.Date,
                   IsAnonymous = c.IsAnonymous,
                   Likes = c.Likes,
                   Dislikes = c.Dislikes
               })
               .ToListAsync();
            return comments;
        }

        // GET: api/contributions/{id}/comment
        [HttpGet("{id}/commentofcomment")]
        public async Task<ActionResult<IEnumerable<CommentOfCommentDTO>>> GetCommentOfComment(int id)
        {
            var comments = await _context.CommentOfComments
               .Where(c => c.CommentId == id)
               .Select(c => new CommentOfCommentDTO
               {
                   Id = c.Id,
                   CommentId = c.CommentId,
                   UserId = c.UserId,
                   Content = c.Content,
                   Date = c.Date,
                   IsAnonymous = c.IsAnonymous
               })
               .ToListAsync();
            return comments;
        }
        // POST: api/contributions
        [HttpPost]
        [Authorize(Roles = "Student")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ContributionPostDTO>> PostContribution([FromForm] ContributionPostDTO contributionDTO, IFormFile file)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)        
            {
                return Unauthorized();
            }
            if (file != null)
            {
                if (!IsFileValid(file))
                {
                    return BadRequest("Invalid file format. Only Word documents (doc, docx) and images (jpg, jpeg, png, bmp) are allowed.");
                }
            }
            byte[] fileContent = null;
            string contentType = null;
            if (file != null)
            {
                using (var ms = new MemoryStream()) 
                {
                    await file.CopyToAsync(ms);
                    fileContent = ms.ToArray();
                    contentType = file.ContentType;
                }
            }
            ContributionStatus status =(ContributionStatus)Enum.Parse(typeof(ContributionStatus) ,contributionDTO.Status.ToString());
            var contribution = new Contribution
            {
                Title = contributionDTO.Title,
                SubmissionDate = DateTime.Now,
                Content = contributionDTO.Content,
                SelectedForPublication = contributionDTO.SelectedForPublication,
                Commented = false,
                Likes = contributionDTO.Likes,
                Dislikes = contributionDTO.Dislikes,
                Views = contributionDTO.Views,
                Status = status,
                UserID = currentUser.Id, // Set UserID from current user
                FacultyID = currentUser.FacultyID,
                EventID = contributionDTO.EventID
            };
            _context.Contributions.Add(contribution);
            await _context.SaveChangesAsync();
            if (fileContent != null && !string.IsNullOrEmpty(contentType))
            {
                var uploadedDocument = new UploadedDocument
                {
                    ContributionId = contribution.ContributionID,
                    Content = fileContent,
                    ContentType = contentType
                };
                _context.UploadedDocuments.Add(uploadedDocument);
                await _context.SaveChangesAsync();
            }
            var coordinators = await _userManager.GetUsersInRoleAsync("MarketingCoordinator");
            foreach (var coordinator in coordinators)
            {
                var subject = "Notification of new student posts";
                var title = contribution.Title;
                var author = currentUser.UserName;
                var dateSent = DateTime.Now.ToString("yyyy-MM-dd"); 
                var message = $@"<p>Subject: {subject}</p>
                            <br/>
                            <p>Here are the details of the article:</p>
                            <br/>
                            <p>Title: {title}</p>
                            <p>Author: {author}</p>
                            <p>Date sent: {dateSent}</p>
                            <br/>
                            <p>Please take a moment to review the post and ensure it meets our guidelines and standards. Your feedback and approval are greatly appreciated.</p>
                            <br/>
                            <p>Thank you for your attention to this issue.</p>";
                await _emailService.SendEmailAsync(coordinator.Email, subject, message);
            }

            return CreatedAtAction("GetContribution", new { id = contribution.ContributionID }, contributionDTO);
        }
        private bool IsFileValid(IFormFile file)
        {
            var allowedExtensions = new[] { ".doc", ".docx", ".jpg", ".jpeg", ".png", ".bmp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            return allowedExtensions.Contains(fileExtension);
        }

        // PUT: api/contributions/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Student")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PutContribution(int id, [FromForm] ContributionPostDTO contributionDTO, IFormFile file)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }
            var contribution = await _context.Contributions.FindAsync(id);
            if (contribution == null)
            {
                return NotFound();
            }

            if (contribution.UserID != currentUser.Id)
            {
                return Forbid();
            }
            ContributionStatus status = (ContributionStatus)Enum.Parse(typeof(ContributionStatus), contributionDTO.Status.ToString());
            contribution.Title = contributionDTO.Title;
            contribution.SubmissionDate = DateTime.Now;
            contribution.Content = contributionDTO.Content;
            contribution.SelectedForPublication = contributionDTO.SelectedForPublication;
            contribution.Commented = contributionDTO.Commented;
            contribution.Likes = contributionDTO.Likes;
            contribution.Dislikes = contributionDTO.Dislikes;
            contribution.Views = contributionDTO.Views;
            contribution.Status = status;
            contribution.FacultyID = currentUser.FacultyID;
            contribution.EventID = contributionDTO.EventID;
            try
            {
                _context.Entry(contribution).State = EntityState.Modified;
                if (file != null)
                {
                    byte[] fileContent = null;
                    using (var ms = new MemoryStream())
                    {
                        await file.CopyToAsync(ms);
                        fileContent = ms.ToArray();
                    }
                    var uploadedDocument = await _context.UploadedDocuments.FirstOrDefaultAsync(d => d.ContributionId == contribution.ContributionID);
                    if (uploadedDocument == null)
                    {
                        uploadedDocument = new UploadedDocument
                        {
                            ContributionId = contribution.ContributionID,
                            Content = fileContent,
                            ContentType = file.ContentType
                        };
                        _context.UploadedDocuments.Add(uploadedDocument);
                    }
                    else
                    {
                        uploadedDocument.Content = fileContent;
                        uploadedDocument.ContentType = file.ContentType;
                        _context.Entry(uploadedDocument).State = EntityState.Modified;
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContributionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        // PUT: api/contributions/{id}
        [HttpPut("contributionAgree/{id}")]
        [Authorize(Roles = "MarketingCoordinator")]
        public async Task<IActionResult> PutContributionAgree(int id, bool agree, bool publication, int newstatus, string comment)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }
            var contribution = await _context.Contributions.FindAsync(id);
            if (contribution == null)
            {
                return NotFound();
            }

            if (contribution.UserID != currentUser.Id)
            {
                return Forbid();
            }
            ContributionStatus status = (ContributionStatus)Enum.Parse(typeof(ContributionStatus), newstatus.ToString());
            contribution.SelectedForPublication = publication;
            contribution.Commented = agree;
            contribution.Status = status;
            contribution.FacultyID = currentUser.FacultyID;

            try
            {
                _context.Entry(contribution).State = EntityState.Modified;
                if(comment != "")
                {
                    var student = await _userManager.FindByIdAsync(contribution.UserID);
                    var subject = "Notification of contributions ";
                    var author = student.UserName;
                    var title = contribution.Title;
                    var dateSent = DateTime.Now.ToString("yyyy-MM-dd");
                    var pub = publication == true ? "Yes" : "No";
                    var sts = status.ToString();
                    var message = $@"<p>Subject: {subject}</p>
                            <br/>
                            <p>Here are the contribution's evaluation details:</p>
                            <br/>
                            <p>Author: {author}</p>
                            <p>Date sent: {dateSent}</p>
                            <br/>
                            <p>Publication : {pub}<p>
                            <p>Status: {sts}<p>
                            <br/>
                            <p>Comment : {comment}</p>
                            <br/>
                            <p>Thank you for your attention to this issue.</p>";
                    await _emailService.SendEmailAsync(student.Email, subject, message);
                }
                
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContributionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        // DELETE: api/contributions/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> DeleteContribution(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var contribution = await _context.Contributions.FindAsync(id);
            if (contribution == null)
            {
                return NotFound();
            }

            if (contribution.UserID != currentUser.Id)
            {
                return Forbid();
            }

            _context.Contributions.Remove(contribution);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ContributionExists(int id)
        {
            return _context.Contributions.Any(e => e.ContributionID == id);
        }
    }
}
