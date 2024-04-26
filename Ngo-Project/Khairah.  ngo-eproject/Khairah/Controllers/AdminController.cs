using Microsoft.AspNetCore.Mvc;
using Khairah_.Models;
using Khairah_.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Stripe.Radar;

namespace Khairah_.Controllers
{

    public class AdminController : Controller
    {




        private readonly NgoProjectdbContext db;


        public AdminController(NgoProjectdbContext db)
        {
            this.db = db;
        }


        [Authorize]
        public IActionResult Index()
        {
            var allRecord = new IndexRecord
            {
                Sponsors = db.OurSponsors.ToList(),
                Ngovolunteers = db.NgoVolunteers.ToList(),
                Cause = db.Causes.ToList(),
                Events = db.NgoEvents.ToList(),
                Donations = db.Donations.ToList(),
                volrequest = db.Volunteers.ToList(),

                contacts = db.Contacts.ToList()


            };

            return View(allRecord);
        }


        //==================Login Sign Up Start===============//
        public IActionResult admin_lockscreen()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(User user)
        {
            try
            {
                var data = db.Users.FirstOrDefault(x => x.UserEmail == user.UserEmail && x.UserPassword == user.UserPassword);
                if (data != null)
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, data.UserName),
                new Claim(ClaimTypes.Email, data.UserEmail),
                new Claim(ClaimTypes.Role, data.UserRoleId == 1 ? "Admin" : "User"),
                new Claim("UserImage", data.UserImage ?? "us-icon.png") 
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    TempData["Success"] = data.UserRoleId == 1 ? "Admin login successful!" : "User login successful!";

                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    TempData["Error"] = "Invalid Password.";
                    return View("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while processing your request. Please try again later. " + ex;
                return View("Login");
            }
        }



        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction(nameof(Index), "Admin");
        }




        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult admin_register()
        {
            return View();
        }

        //==================Login Sign Up End===============//

        //==================Cause Type Mapping Start===============//


        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult Cause_T_add() // insert
        {
            return View();
        }


        public IActionResult addCauseType(CauseType causetype) // insert
        {
            try
            {
                db.Add(causetype);
                db.SaveChanges();
                TempData["Success"] = "Cause Type Insert SuccessFully";
                return RedirectToAction(nameof(Cause_T_add));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while inserting the record. Please try again later. " + ex.Message;
                return RedirectToAction(nameof(Cause_T_add));
            }
        }


        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult Cause_T_show() //read
        {
            return View(db.CauseTypes.ToList());
        }


        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult Cause_T_del(int? id) // delete
        {
            try
            {
                var delete = db.CauseTypes.FirstOrDefault(ct => ct.CTypeId == id);
                if (delete == null)
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction(nameof(Cause_T_show));
                }

                db.Remove(delete);
                db.SaveChanges();
                TempData["Success"] = "Record Deleted Successfully..";
                return RedirectToAction(nameof(Cause_T_show));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the record. Please try again later." + ex.Message;
                return RedirectToAction(nameof(Cause_T_show));
            }
        }


        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult Cause_T_edit(int? id) //edit
        {
            try
            {
                var data = db.CauseTypes.FirstOrDefault(ct => ct.CTypeId == id);
                if (data == null)
                {
                    TempData["Error"] = "Cause Type not found.";
                    return RedirectToAction(nameof(Cause_T_show));
                }
                return View(data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading the cause type for editing: " + ex.Message;
                return RedirectToAction(nameof(Cause_T_show));
            }

        }
        public IActionResult Cause_T_edit2(CauseType causetype) //update
        {
            try
            {
                var originalCauseType = db.CauseTypes.AsNoTracking().FirstOrDefault(ct => ct.CTypeId == causetype.CTypeId);
                if (originalCauseType == null)
                {
                    TempData["Error"] = "Cause Type not found.";
                    return RedirectToAction(nameof(Cause_T_show));
                }

                causetype.CCreatedDate = originalCauseType.CCreatedDate;

                db.Update(causetype);
                db.SaveChanges();

                TempData["Success"] = "Records Updated Successfully!!";
                return RedirectToAction(nameof(Cause_T_show));
            }

            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating the category: " + ex.Message;
                return RedirectToAction(nameof(Cause_T_show));
            }
        }


        //==================Cause Type Mapping End===============//



        //==================Cause Start=============================//


        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult Cause_add() // insert view
        {
            ViewBag.CauseType = new SelectList(db.CauseTypes, "CTypeId", "CTypeName");
            return View();
        } 
               
        public IActionResult Add_Cause(Cause cause, IFormFile c_img) // insert
        {
            try
            {
                if (cause != null)
                {
                    if (c_img != null && c_img.Length > 0)
                    {
                        var filename = Path.GetFileName(c_img.FileName);
                        string folderPath = Path.Combine("wwwroot/Contentimages/cause", filename);
                        var dbpath = Path.Combine("cause", filename);
                        using (var stream = new FileStream(folderPath, FileMode.Create))
                        {
                            c_img.CopyTo(stream);
                        }
                        cause.CImage = dbpath;
                        
                    }
                    db.Add(cause);
                    db.SaveChanges();
                    TempData["Success"] = "Cause Record Insert Successfully..";
                    return RedirectToAction(nameof(Cause_show));
                }
                else
                {
                    TempData["Error"] = "Invalid cause data. Please check your input and try again.";
                    return View(cause);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while adding the cause. Please try again later. " + ex.Message;
                return View(cause);
            }
        }


        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult Cause_show() // read
        {

            return View(db.Causes.Include("CauseType").ToList());

        }

        public IActionResult cause_del(int? id)  //delete
        {
            try
            {
                var delete = db.Causes.FirstOrDefault(x => x.CId == id);
                if (delete == null)
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction(nameof(public_feedback));
                }

                db.Remove(delete);
                db.SaveChanges();
                TempData["Success"] = "Record Deleted Successfully..";
                return RedirectToAction(nameof(Cause_show));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the record. Please try again later." + ex.Message;
                return RedirectToAction(nameof(Cause_show));
            }
        }


        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult Cause_edit(int? id) // update
        {
            var cause_data = db.Causes.FirstOrDefault(c => c.CId == id);
            ViewBag.CauseType = new SelectList(db.CauseTypes, "CTypeId", "CTypeName");
            return View(cause_data);
        }

        public IActionResult cause_edit2(Cause cause, IFormFile cause_file) //update
        {
            try
            {
                if (cause_file != null && cause_file.Length > 0)
                {
                    Guid r = Guid.NewGuid();
                    var filename = Path.GetFileNameWithoutExtension(cause_file.FileName);
                    var extension = cause_file.ContentType.ToLower();
                    var exten_presize = extension.Substring(6);

                    var unique_name = filename + r + "." + exten_presize;
                    string folderPath = Path.Combine(HttpContext.Request.PathBase.Value, "wwwroot/Contentimages/cause");
                    var dbpath = Path.Combine(folderPath, unique_name);
                    using (var stream = new FileStream(dbpath, FileMode.Create))
                    {
                        cause_file.CopyTo(stream);
                    }
                    var dbAddress = Path.Combine(unique_name);

                    cause.CImage = dbAddress; // Update image path in cause
                    db.Update(cause);
                    db.SaveChanges();
                    TempData["Success"] = "Record Update SuccessFully";
                    return RedirectToAction(nameof(Cause_show));
                }
                else
                {
                    var existingcause = db.Causes.FirstOrDefault(c =>c.CId  == cause.CId);
                    if (existingcause != null)
                    {
                        existingcause.CName = cause.CName;
                        existingcause.CDesc = cause.CDesc;
                        existingcause.CGoalAmount = cause.CGoalAmount;
                        existingcause.CauseType = cause.CauseType;
                        existingcause.CauseTypeId = cause.CauseTypeId;

                        db.SaveChanges();
                        TempData["Success"] = "Record Updated Successfully with previous image";
                        return RedirectToAction(nameof(Cause_show));
                    }
                    else
                    {
                        TempData["Error"] = "Record Not Found";
                        return RedirectToAction(nameof(Cause_show));
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating the Cause: " + ex.Message;
                return RedirectToAction(nameof(Cause_show));
            }
        }



        //==================Cause End===============//


        //==================donation_show Start===============//


        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult donation_show()
        {
            return View(db.Donations.Include(d => d.DCause).ToList());


        }
        //==================donation_show End===============//

        //==================Event Start===============//


        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult event_add()
        {
            return View();
        }

        public IActionResult add_event(NgoEvent nevent, IFormFile eventimg)
        {
            try
            {
                if (nevent != null)
                {
                    if (eventimg != null && eventimg.Length > 0)
                    {
                        var filename = Path.GetFileName(eventimg.FileName);
                        string folderPath = Path.Combine("wwwroot/Contentimages/event", filename);
                        var dbpath = Path.Combine("event", filename);
                        using (var stream = new FileStream(folderPath, FileMode.Create))
                        {
                            eventimg.CopyTo(stream);
                        }
                        nevent.EventImage = dbpath;

                    }
                    db.Add(nevent);
                    db.SaveChanges();
                    TempData["Success"] = "Event added successfully.";
                    return RedirectToAction(nameof(event_show));
                }
                else
                {
                    TempData["Error"] = "Invalid event data. Please check your input and try again.";
                    return View(nevent);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while adding the event. Please try again later. " + ex.Message;
                return View(nevent);
            }
        }


        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult event_show()
        {
            return View(db.NgoEvents.ToList());
        }

        public IActionResult event_del(int? id) //delete
        {
            try
            {
                var delete = db.NgoEvents.FirstOrDefault(e => e.EventId == id);
                if (delete == null)
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction(nameof(event_show));
                }

                db.Remove(delete);
                db.SaveChanges();
                TempData["Success"] = "Record Deleted Successfully..";
                return RedirectToAction(nameof(event_show));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the record. Please try again later." + ex.Message;
                return RedirectToAction(nameof(event_show));
            }
        }


        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult event_edit(int? id) // update
        {
            try
            {
                var data = db.NgoEvents.FirstOrDefault(ne => ne.EventId == id);
                if (data == null)
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction(nameof(event_show));
                }
                return View(data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading the Event Record for editing: " + ex.Message;
                return RedirectToAction(nameof(event_show));
            }
        }

        public IActionResult event_edit2(NgoEvent ngoevent, IFormFile event_file) //update
        {
            try
            {
                if (event_file != null && event_file.Length > 0)
                {
                    Guid r = Guid.NewGuid();
                    var filename = Path.GetFileNameWithoutExtension(event_file.FileName);
                    var extension = event_file.ContentType.ToLower();
                    var exten_presize = extension.Substring(6);

                    var unique_name = filename + r + "." + exten_presize;
                    string folderPath = Path.Combine(HttpContext.Request.PathBase.Value, "wwwroot/Contentimages/event");
                    var dbpath = Path.Combine(folderPath, unique_name);
                    using (var stream = new FileStream(dbpath, FileMode.Create))
                    {
                        event_file.CopyTo(stream);
                    }
                    var dbAddress = Path.Combine(unique_name);

                    ngoevent.EventImage = dbAddress; // Update image path in cause
                    db.Update(ngoevent);
                    db.SaveChanges();
                    TempData["Success"] = "Record Update SuccessFully";
                    return RedirectToAction(nameof(event_show));
                }
                else
                {
                    var existingevent = db.NgoEvents.FirstOrDefault(e => e.EventId == ngoevent.EventId);
                    if (existingevent != null)
                    {
                        existingevent.EventTitle = ngoevent.EventTitle;
                        existingevent.EventDescription = ngoevent.EventDescription;
                        existingevent.EventLocation = ngoevent.EventLocation;
                        existingevent.EventDate = ngoevent.EventDate;
                        existingevent.EventTimeStart = ngoevent.EventTimeStart;
                        existingevent.EventTimeEnd = ngoevent.EventTimeEnd;

                        db.SaveChanges();
                        TempData["Success"] = "Record Updated Successfully with previous image";
                        return RedirectToAction(nameof(event_show));
                    }
                    else
                    {
                        TempData["Error"] = "Record Not Found";
                        return RedirectToAction(nameof(event_show));
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating the Cause: " + ex.Message;
                return RedirectToAction(nameof(event_show));
            }
        }






        //==================Event End===============//

        //==================Partners Start===============//


        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult Partner_add()
        {
            return View();
        }
        public IActionResult add_Partner(OurSponsor sponsor, IFormFile sponimg)
        {
            try
            {
                if (sponsor != null)
                {
                    if (sponimg != null && sponimg.Length > 0)
                    {
                        var filename = Path.GetFileName(sponimg.FileName);
                        string folderPath = Path.Combine("wwwroot/Contentimages/partners", filename);
                        var dbpath = Path.Combine("partners", filename);
                        using (var stream = new FileStream(folderPath, FileMode.Create))
                        {
                            sponimg.CopyTo(stream);
                        }
                        sponsor.SponsorLogo = dbpath;

                    }
                    db.Add(sponsor);
                    db.SaveChanges();
                    TempData["Success"] = "Sponsor added successfully.";
                    return RedirectToAction(nameof(Partner_show));
                }
                else
                {
                    TempData["Error"] = "Invalid Partners data. Please check your input and try again.";
                    return View(sponsor);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while adding the Sponsor. Please try again later. " + ex.Message;
                return View(sponsor);
            }
        }


        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult Partner_show()
        {

            return View(db.OurSponsors.ToList());
        }


        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult Partner_edit(int? id)
        {
            try
            {
                var data = db.OurSponsors.FirstOrDefault(os => os.SponsorId == id);
                if (data == null)
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction(nameof(Partner_show));
                }
                return View(data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading the Sponsor for editing: " + ex.Message;
                return RedirectToAction(nameof(Partner_show));
            }
        }
        public IActionResult Partner_edit2(OurSponsor sponsor, IFormFile sponfile)
        {
            try
            {
                if (sponfile != null && sponfile.Length > 0)
                {
                    Guid r = Guid.NewGuid();
                    var filename = Path.GetFileNameWithoutExtension(sponfile.FileName);
                    var extension = sponfile.ContentType.ToLower();
                    var exten_presize = extension.Substring(6);

                    var unique_name = filename + r + "." + exten_presize;
                    string folderPath = Path.Combine(HttpContext.Request.PathBase.Value, "wwwroot/Contentimages/partners");
                    var dbpath = Path.Combine(folderPath, unique_name);
                    using (var stream = new FileStream(dbpath, FileMode.Create))
                    {
                        sponfile.CopyTo(stream);
                    }
                    var dbAddress = Path.Combine(unique_name);

                    sponsor.SponsorLogo = dbAddress; // Update image path in cause
                    sponsor.SponsorshipDate = DateTime.Now;
                    db.Update(sponsor);
                    db.SaveChanges();
                    TempData["Success"] = "Record Update SuccessFully";
                    return RedirectToAction(nameof(Partner_show));
                }
                else
                {
                    var existingsponsor = db.OurSponsors.FirstOrDefault(os => os.SponsorId == sponsor.SponsorId);
                    if (existingsponsor != null)
                    {
                        existingsponsor.SponsorName = sponsor.SponsorName;
                        existingsponsor.SponsorshipDate = DateTime.Now;

                        db.SaveChanges();
                        TempData["Success"] = "Record Updated Successfully with previous image";
                        return RedirectToAction(nameof(Partner_show));
                    }
                    else
                    {
                        TempData["Error"] = "Record Not Found";
                        return RedirectToAction(nameof(Partner_show));
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating the Partner: " + ex.Message;
                return RedirectToAction(nameof(Partner_show));
            }
        }
        public IActionResult Partner_del(int? id)
        {
            try
            {
                var delete = db.OurSponsors.FirstOrDefault(os => os.SponsorId == id);
                if (delete == null)
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction(nameof(Partner_show));
                }

                db.Remove(delete);
                db.SaveChanges();
                TempData["Success"] = "Record Deleted Successfully..";
                return RedirectToAction(nameof(Partner_show));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the record. Please try again later." + ex.Message;
                return RedirectToAction(nameof(Partner_show));
            }
        }
        //==================Partners End===============//

        //==================TimeLine Start===============//


        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult public_feedback() //read
        {
            return View(db.Contacts.ToList());
            
        }

        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult cont_del(int? id)  //delete
        {
            try
            {
                var delete = db.Contacts.FirstOrDefault(x => x.ContId == id);
                if (delete == null)
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction(nameof(public_feedback));
                }

                db.Remove(delete);
                db.SaveChanges();
                TempData["Success"] = "Record Deleted Successfully..";
                return RedirectToAction(nameof(public_feedback));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the record. Please try again later." + ex.Message;
                return RedirectToAction(nameof(public_feedback));
            }
        }



        //==================TimeLine End===============//

        //==================Role Start===============//


        [Authorize(Roles = "Admin")]
        public IActionResult Role_add() //insert
        {
            return View();
        }


        [Authorize(Roles = "Admin")]
        public IActionResult addRole(UserRole role) //insert
        {
            try
            {
                db.Add(role);
                db.SaveChanges();
                TempData["Success"] = "Admin Role Insert SuccessFully";
                return RedirectToAction(nameof(Role_add));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while inserting the record. Please try again later. " + ex.Message;
                return RedirectToAction(nameof(Role_add));
            }
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Role_show() //read
        {
            return View(db.UserRoles.ToList());
        }

        public IActionResult role_del(int? id) //delete
        {
            try
            {
                var delete = db.UserRoles.FirstOrDefault(x => x.RoleId == id);
                if (delete == null)
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction(nameof(Role_show));
                }

                db.Remove(delete);
                db.SaveChanges();
                TempData["Success"] = "Record Deleted Successfully..";
                return RedirectToAction(nameof(Role_show));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the record. Please try again later." + ex.Message;
                return RedirectToAction(nameof(Role_show));
            }
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Role_edit(int? id) //update
        {
            try
            {
                var data = db.UserRoles.FirstOrDefault(x => x.RoleId == id);
                if (data == null)
                {
                    TempData["Error"] = "Admin Role not found.";
                    return RedirectToAction(nameof(Role_show));
                }
                return View(data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading the category for editing: " + ex.Message;
                return RedirectToAction(nameof(Role_show));
            }

        }
        public IActionResult Role_edit2(UserRole role) //update
        {
            try
            {
                db.Update(role);
                db.SaveChanges();
                TempData["Success"] = "Records Updated Successfully!!";
                return RedirectToAction(nameof(Role_show));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating the category: " + ex.Message;
                return RedirectToAction(nameof(Role_show));
            }
        }

        //==================Role End===============//

        //==================User Start===============//


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult user_add() //insert
        {

            ViewBag.UserRoles = new SelectList(db.UserRoles, "RoleId", "RoleName");
            return View();
        }

        [HttpPost]
        public IActionResult user_add(User user, IFormFile uimg) //insert
        {
            try
            {
                if (user != null)
                {
                    if (uimg != null && uimg.Length > 0)
                    {
                        var filename = Path.GetFileName(uimg.FileName);
                        string folderPath = Path.Combine("wwwroot/Contentimages/profile", filename);
                        var dbpath = Path.Combine("profile", filename);
                        using (var stream = new FileStream(folderPath, FileMode.Create))
                        {
                            uimg.CopyTo(stream);
                        }
                        user.UserImage = dbpath;
                        user.UserRoleId = 2;
                    }
                    db.Add(user);
                    db.SaveChanges();
                    TempData["Success"] = "User Registered Successfully..";
                    return RedirectToAction(nameof(user_show));
                }
                else
                {
                    TempData["Error"] = "Invalid user data. Please check your input and try again.";
                    return View(user);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while registering the user. Please try again later. " + ex.Message;
                return View(user);
            }
        }


        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult user_show() // read
        {
            return View(db.Users.Include("UserRole").ToList());

        }
        [Authorize(Roles = "Admin")]
        public IActionResult user_del(int? id) // delete
        {
            try
            {
                var delete = db.Users.FirstOrDefault(u => u.UserId == id);
                if (delete == null)
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction(nameof(user_show));
                }

                if (delete.UserRoleId == 1)
                {
                    TempData["Error"] = "You cannot delete the admin user.";
                    return RedirectToAction(nameof(user_show));
                }
                else if (delete.UserRoleId > 1)
                {
                    TempData["Success"] = "User Record Deleted Successfully..";
                }

                db.Remove(delete);
                db.SaveChanges();

                return RedirectToAction(nameof(user_show));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the record. Please try again later. " + ex.Message;
                return RedirectToAction(nameof(user_show));
            }
        }



        [Authorize(Roles = "Admin")]
        public IActionResult user_edit(int? id) // update
        {
            var user_data = db.Users.FirstOrDefault(u => u.UserId == id);
            ViewBag.UserRoles = new SelectList(db.UserRoles, "RoleId", "RoleName");
            return View(user_data);
        }

        public IActionResult user_edit2(User user, IFormFile file) //update
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    Guid r = Guid.NewGuid();
                    var filename = Path.GetFileNameWithoutExtension(file.FileName);
                    var extension = file.ContentType.ToLower();
                    var exten_presize = extension.Substring(6);

                    var unique_name = filename + r + "." + exten_presize;
                    string folderPath = Path.Combine(HttpContext.Request.PathBase.Value, "wwwroot/Contentimages/profile");
                    var dbpath = Path.Combine(folderPath, unique_name);
                    using (var stream = new FileStream(dbpath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    var dbAddress = Path.Combine(unique_name);

                    user.UserImage = dbAddress;
                    db.Update(user);
                    db.SaveChanges();
                    TempData["Success"] = "Record Update SuccessFully";
                    return RedirectToAction(nameof(user_show));

                }
                else
                {                    
                    var existinguser = db.Users.FirstOrDefault(u => u.UserId == user.UserId);
                    if (existinguser != null)
                    {
                        existinguser.UserRoleId = user.UserRoleId; 
                        existinguser.UserName = user.UserName;
                        existinguser.UserEmail = user.UserEmail;
                        existinguser.UserPassword = user.UserPassword;
                        existinguser.UserRole = user.UserRole;

                        db.SaveChanges();
                        TempData["Success"] = "Record Updated Successfully with previous image";
                        return RedirectToAction(nameof(user_show));
                    }
                    else
                    {
                        TempData["Error"] = "Record Not Found";
                        return RedirectToAction(nameof(user_show));
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating the User: " + ex.Message;
                return RedirectToAction(nameof(user_show));
            }
        }



        //==================User End===============//





        //==================Volunteer Start===============//



        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult vol_add() // insert
        {
            return View();

        }

        public IActionResult add_vol(NgoVolunteer ngovolunteer, IFormFile nvimg) // insert
        {
            try
            {
                if (ngovolunteer != null)
                {
                    if (nvimg  != null && nvimg.Length > 0)
                    {
                        var filename = Path.GetFileName(nvimg.FileName);
                        string folderPath = Path.Combine("wwwroot/Contentimages/team", filename);
                        var dbpath = Path.Combine("test", filename);
                        using (var stream = new FileStream(folderPath, FileMode.Create))
                        {
                            nvimg.CopyTo(stream);
                        }
                        ngovolunteer.NvImg = dbpath;
                    }
                    db.Add(ngovolunteer);
                    db.SaveChanges();
                    TempData["Success"] = "Ngo Volunteer Registered Successfully..";
                    return RedirectToAction(nameof(vol_show));
                }
                else
                {
                    TempData["Error"] = "Invalid Ngo Volunteer data. Please check your input and try again.";
                    return View(ngovolunteer);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while registering the Ngo Volunteer. Please try again later. " + ex.Message;
                return View(ngovolunteer);
            }

        }


        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult vol_show() //read
        {
             return View(db.NgoVolunteers.ToList());
            
        }

        public IActionResult ngo_vol_del(int? id) //delete
        {
            try
            {
                var delete = db.NgoVolunteers.FirstOrDefault(nv => nv.NvId == id);
                if (delete == null)
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction(nameof(vol_show));
                }

                db.Remove(delete);
                db.SaveChanges();
                TempData["Success"] = "Record Deleted Successfully..";
                return RedirectToAction(nameof(vol_show));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the record. Please try again later." + ex.Message;
                return RedirectToAction(nameof(vol_show));
            }
        }



        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult vol_edit(int? id) // update
        {
            try
            {
                var data = db.NgoVolunteers.FirstOrDefault(ngv => ngv.NvId == id);
                if (data == null)
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction(nameof(vol_show));
                }
                return View(data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading the category for editing: " + ex.Message;
                return RedirectToAction(nameof(vol_show));
            }
        }

        public IActionResult vol_edit2(NgoVolunteer ngvol, IFormFile vol_file) //update
        {
            try
            {
                if (vol_file != null && vol_file.Length > 0)
                {
                    Guid r = Guid.NewGuid();
                    var filename = Path.GetFileNameWithoutExtension(vol_file.FileName);
                    var extension = vol_file.ContentType.ToLower();
                    var exten_presize = extension.Substring(6);

                    var unique_name = filename + r + "." + exten_presize;
                    string folderPath = Path.Combine(HttpContext.Request.PathBase.Value, "wwwroot/Contentimages/team");
                    var dbpath = Path.Combine(folderPath, unique_name);
                    using (var stream = new FileStream(dbpath, FileMode.Create))
                    {
                        vol_file.CopyTo(stream);
                    }
                    var dbAddress = Path.Combine(unique_name);

                    ngvol.NvImg = dbAddress; // Update image path in cause
                    ngvol.NvCreatedDatetime = DateTime.Now;
                    db.Update(ngvol);
                    db.SaveChanges();
                    TempData["Success"] = "Record Update SuccessFully";
                    return RedirectToAction(nameof(vol_show));
                }
                else
                {
                    var existingvol = db.NgoVolunteers.FirstOrDefault(ngv => ngv.NvId == ngvol.NvId);
                    if (existingvol != null)
                    {
                        existingvol.NvName = ngvol.NvName;
                        existingvol.NvEmail = ngvol.NvEmail;
                        existingvol.NvCreatedDatetime = DateTime.Now;
                        

                        db.SaveChanges();
                        TempData["Success"] = "Record Updated Successfully with previous image";
                        return RedirectToAction(nameof(vol_show));
                    }
                    else
                    {
                        TempData["Error"] = "Record Not Found";
                        return RedirectToAction(nameof(vol_show));
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating the Cause: " + ex.Message;
                return RedirectToAction(nameof(vol_show));
            }
        }




        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult vol_records() //read
        {
            return View(db.Volunteers.ToList());

        }

        [Authorize(Roles = "Admin , Volunteer")]
        public IActionResult vol_del(int? id)  //delete
        {
            try
            {
                var delete = db.Volunteers.FirstOrDefault(x => x.VId == id);
                if (delete == null)
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction(nameof(vol_records));
                }

                db.Remove(delete);
                db.SaveChanges();
                TempData["Success"] = "Record Deleted Successfully..";
                return RedirectToAction(nameof(vol_records));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the record. Please try again later." + ex.Message;
                return RedirectToAction(nameof(vol_records));
            }
        }
    }



    //==================Volunteer End===============//





}

