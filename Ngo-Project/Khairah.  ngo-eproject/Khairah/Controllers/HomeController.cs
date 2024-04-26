using Khairah_.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Khairah_.Data;
using System.Net.Mail;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Rendering;
using Stripe;
using Stripe.Checkout;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;



namespace Khairah_.Controllers
{

    public class HomeController : Controller
    {
        

        private readonly NgoProjectdbContext db;

        private readonly StripeSetting _stripSetting;
        public HomeController(NgoProjectdbContext db, IOptions<StripeSetting> stripSetting)
        {
            this.db = db;
            _stripSetting = stripSetting.Value;

        }


        //==========================================================================Index Start==============================================================================//
        public IActionResult Index()
        {
            var allRecord = new IndexRecord
            {
                Sponsors = db.OurSponsors.ToList(),
                Ngovolunteers = db.NgoVolunteers.ToList(),
                Cause = db.Causes.ToList(),
                Events = db.NgoEvents.ToList()
                
            };

            return View(allRecord);
        }

        //==========================================================================End Index=================================================================================//
        //==========================================================================Start About===============================================================================//
        public IActionResult about()
        {
            var allRecord = new IndexRecord
            {
                Sponsors = db.OurSponsors.ToList(),
                Ngovolunteers = db.NgoVolunteers.ToList(),
                Donations = db.Donations.ToList(),

                Cause = db.Causes.ToList()
            };
            return View(allRecord);
        }

        //==========================================================================End About==================================================================================//
        //==========================================================================Start Blog=================================================================================//
        public IActionResult blog()
        {
            return View();
        }

        //==========================================================================End Blog====================================================================================//
        //=========================================================================Causes Start=================================================================================//

        public IActionResult causes()
        {

            return View(db.Causes.Include("CauseType").ToList());
        }

        public IActionResult causes_single(int? id)
        {

            return View(db.Causes.Where(c => c.CauseTypeId == id).Include("CauseType").ToList());
        }
        //=========================================================================Causes End=====================================================================================//
        //=========================================================================Start Contact==================================================================================//

        [Authorize]
        [HttpGet]
        public IActionResult contact()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public IActionResult contact_us(Contact cont)
        {
            try
            {
                if (string.IsNullOrEmpty(cont.ContEmail))
                {
                    ModelState.AddModelError("ContEmail", "Email is required");
                    return View("Contact", cont);
                }

                db.Add(cont);
                db.SaveChanges();

                using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true; // Enable SSL
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("sherryop121@gmail.com", "lfwj wawa xoqt cfub"); 

                    using (MailMessage msg = new MailMessage("sherryop121@gmail.com", cont.ContEmail))
                    {
                        msg.Subject = "Thank you for contacting us!";
                        msg.Body = cont.ContName + ", thank you for contacting us! " ;

                        client.Send(msg);
                    }
                }


                TempData["Success"] = "Your message has been successfully sent! We will get back to you soon.";

                return RedirectToAction(nameof(Contact));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while processing your request. Please try again later. Error Message: " + ex.Message;
                return RedirectToAction("Contact", cont);
            }
        }

        //=============================================================================End Contact========================================================================//
        //========================================================================Donation Start==========================================================================//
        [Authorize]
        public IActionResult donate()
        {
            ViewBag.Cause = new SelectList(db.Causes, "CId", "CName");

            return View();
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddDonation(Donation model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(model.DEmail))
                    {
                        ModelState.AddModelError("DEmail", "Email is required");
                        return View("donate", model);
                    }

                    using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587))
                    {
                        client.EnableSsl = true; // Enable SSL
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential("sherryop121@gmail.com", "lfwj wawa xoqt cfub");

                        using (MailMessage msg = new MailMessage("sherryop121@gmail.com", model.DEmail))
                        {
                            msg.Subject = "Thank You for Your Donation!";
                            string fname = model.DFirstname + ' ' + model.DLastname;
                            msg.Body = $"Dear [{fname}],\nThank you for your generous donation to Khairah. Your support means a lot to us and will make a real difference in the lives of those we serve.\n\nYour kindness and generosity are deeply appreciated. We are grateful for your commitment to making the world a better place.\nBest regards,\nKHAIRAH. COMMUNITY";

                            client.Send(msg);
                        }
                    }

                    StripeConfiguration.ApiKey = _stripSetting.SecretKey;

                    // Create payment session options
                    var options = new SessionCreateOptions
                    {
                        PaymentMethodTypes = new List<string> { "card" },
                        LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmountDecimal = (long)(model.DAmount * 100), // Convert amount to cents
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Donation",
                                Description = "Kharah. Ngo Donation"
                            }
                        },
                        Quantity = 1
                    }
                },
                        Mode = "payment",
                        SuccessUrl = "https://localhost:7085/Home/success",
                        CancelUrl = "https://localhost:7085/Home/cancel"
                    };

                    var service = new SessionService();
                    var session = await service.CreateAsync(options);

                    db.Donations.Add(model);
                    await db.SaveChangesAsync();

                    Cause cause = await db.Causes.FindAsync(model.DCauseId);
                    if (cause != null)
                    {
                        cause.CRaisedAmount += model.DAmount;
                        db.Causes.Update(cause);
                        await db.SaveChangesAsync();
                    }

                    TempData["Warning"] = "Your donation information has been saved. We look forward to receiving your contribution. Please proceed with the payment process to complete your donation.";
                    return Redirect(session.Url);
                }
                else
                {
                    TempData["Error"] = "Validation failed. Please check your input and try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
            }

            ViewBag.Cause = new SelectList(db.Causes, "CId", "CName");
            return View("donate", model);
        }



        public async Task<IActionResult> success()
        {
            ViewBag.Cause = new SelectList(db.Causes, "CId", "CName");
            ViewBag.PaymentStatus = "Payment transferred successfully!";
            TempData["Success"] = "Your payment was successful!";
            return View("donate");
        }

        public IActionResult cancel()
        {
            ViewBag.Cause = new SelectList(db.Causes, "CId", "CName");
            ViewBag.PaymentStatus = "Payment cancelled.";
            TempData["Error"] = "Your payment was cancelled.";
            return View("donate");
        }

        //=====================================================================================Donation End===============================================================//
        //=====================================================================================404 Error==================================================================//
        public IActionResult error()
        {
            return View();
        }

        //=====================================================================================Events Start===============================================================//
        public IActionResult Khairah_event()
        {
            return View(db.NgoEvents.ToList());
        }

        //=====================================================================================Events End==================================================================//
        //=====================================================================================Partner Start===============================================================//
        public IActionResult partner()
        {

            return View(db.OurSponsors.ToList());
        }
        //=====================================================================================Partner End===============================================================//
        //=====================================================================================Register Start============================================================//


        [HttpGet]
		public IActionResult register()
		{
			return View();
		}

        [HttpPost]
        public IActionResult LoginUser(User user)
        {
            try
            {
                var data = db.Users.FirstOrDefault(x => x.UserEmail == user.UserEmail && x.UserPassword == user.UserPassword);
                if (data != null)
                {
                    ClaimsIdentity identity = null;
                    bool isAuthenticate = false;

                    if (data.UserRoleId == 1)
                    {
                        identity = new ClaimsIdentity(new[]
                        {
                    new Claim(ClaimTypes.Name, data.UserName),
                    new Claim(ClaimTypes.Email, data.UserEmail),
                    new Claim(ClaimTypes.NameIdentifier, data.UserId.ToString()),
                    new Claim(ClaimTypes.Role,"Admin"),
                    new Claim("UserImage", data.UserImage ?? "us-icon.png")
                }, CookieAuthenticationDefaults.AuthenticationScheme);
                        isAuthenticate = true;
                    }
                    else if (data.UserRoleId == 2)
                    {
                        identity = new ClaimsIdentity(new[]
                        {
                    new Claim(ClaimTypes.Name, data.UserName),
                    new Claim(ClaimTypes.Email, data.UserEmail),
                    new Claim(ClaimTypes.NameIdentifier, data.UserId.ToString()),
                    new Claim(ClaimTypes.Role,"User"),
                    new Claim("UserImage", data.UserImage ?? "us-icon.png")
                }, CookieAuthenticationDefaults.AuthenticationScheme);
                        isAuthenticate = true;
                    }
                    else if (data.UserRoleId == 3)
                    {
                        identity = new ClaimsIdentity(new[]
                        {
                    new Claim(ClaimTypes.Name, data.UserName),
                    new Claim(ClaimTypes.Email, data.UserEmail),
                    new Claim(ClaimTypes.NameIdentifier, data.UserId.ToString()),
                    new Claim(ClaimTypes.Role,"Volunteer"),
                    new Claim("UserImage", data.UserImage ?? "us-icon.png")
                }, CookieAuthenticationDefaults.AuthenticationScheme);
                        isAuthenticate = true;
                    }

                    if (isAuthenticate)
                    {
                        var principal = new ClaimsPrincipal(identity);
                        var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                        if (data.UserRoleId == 1) // Admin
                        {
                            TempData["Success"] = "Admin login successful!";
                            return RedirectToAction("Index", "Admin");
                        }
                        else if (data.UserRoleId == 2) // User
                        {
                            TempData["Success"] = "User login successful!";
                            return RedirectToAction("Index", "Home");
                        }
                        else if (data.UserRoleId == 3) // Volunteer
                        {
                            TempData["Success"] = "Volunteer login successful!";
                            return RedirectToAction("Index", "Admin"); 
                        }
                    }

                }
                else
                {
                    TempData["Error"] = "Invalid Password.";
                }

                return View("register");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while processing your request. Please try again later. " + ex;

                return View("register");
            }
        }



        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction(nameof(Index), "Home");
        }


        [HttpPost]
        public IActionResult sign_up(User user)
        {
			try
			{
				user.UserRoleId = 2;
				db.Add(user);
				db.SaveChanges();
				TempData["Success"] = "User Registered Successfully.";
				return RedirectToAction(nameof(register));
			}
			catch (DbUpdateException ex)
			{
				if (ex.InnerException is SqlException sqlException && sqlException.Number == 2601)
				{
					ModelState.AddModelError(nameof(user.UserEmail), "Email address is already registered.");
					return View(nameof(register), user);
				}
				else
				{
					TempData["Error"] = "An error occurred while registering the user. Please try again later.";
					return View(nameof(register), user);
				}
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while registering the user. Please try again later. " + ex.Message;
				return View(nameof(register), user);
			}

		}



        //=================================================================================Partner End============================================================================//
        //=================================================================================Volunteer Start========================================================================//
        [Authorize]
        public IActionResult volunteer()
        {

            return View();
        }
        [Authorize]
        public IActionResult vol_record(Volunteer vol)
        {
            try
            {
                if (string.IsNullOrEmpty(vol.VEmail))
                {
                    ModelState.AddModelError("VEmail", "Email is required");
                    return View("Contact", vol);
                }

                db.Add(vol);
                db.SaveChanges();
                using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true; // Enable SSL
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("sherryop121@gmail.com", "lfwj wawa xoqt cfub");

                    using (MailMessage msg = new MailMessage("sherryop121@gmail.com", vol.VEmail))
                    {
                        msg.Subject = "Thank you for volunteering with us!";
                        msg.Body =  $"Dear [{vol.VName}]\nThank you for your interest in volunteering with us! We appreciate your willingness to contribute your time and effort towards our cause. Volunteers like you play a crucial role in helping us achieve our goals and make a positive impact on our community. We will review your application and get back to you shortly. In the meantime, if you have any questions or need further assistance, feel free to reach out to us. Once again, thank you for your commitment and support.\nBest regards,\nKHAIRAH. COMMUNITY";

                        client.Send(msg);
                    }
                }

                TempData["Success"] = "Your request has been successfully submitted. Thank you for reaching out!";
                return RedirectToAction("volunteer");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while inserting the record. Please try again later. " + ex.Message;
                return RedirectToAction("volunteer");
            }
        }

        //=====================================================================================Volunteer End===============================================================//
        //==================================================================================END CODE=======================================================================//


        //===========================================================================================================================================================================//
        //===========================================================================================================================================================================//
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
