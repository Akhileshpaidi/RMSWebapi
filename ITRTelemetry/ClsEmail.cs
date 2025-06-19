using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using MySql.Data.MySqlClient;
using iText.Commons.Utils;
using NuGet.Protocol.Plugins;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ITR_TelementaryAPI
{
    
    public class ClsEmail 
    {
        private string smtpAddress = "smtp.office365.com";
        private int portNumber = 587;
        private bool enableSSL = true;
        private string emailFromAddress = "corporate@neemus.com";
        private string password = "N33mu$123";
        //private string smtpAddress = "smtp.office365.com";
        //private int portNumber = 587;
        //private bool enableSSL = true;
        //private string emailFromAddress = "pfs.grc@ptcfinancial.com";
        //private string password = "Naveen@12345";
       // private string clienturl = "https://grcgov.pfs-ess.com"; //pfs client
        private string clienturl = "http://grma.neemus.com";
        //private readonly string clienturl;
        // Static property to hold the current HttpContext
        public static HttpContext CurrentHttpContext =>
            new HttpContextAccessor().HttpContext;
      //public ClsEmail(IConfiguration configuration)
      //  {
      //      clienturl = configuration["appsettings:Clienturl"];
      //  }
  

        // configuration
        //public static IConfiguration Configuration { get; }
        public static string Connection = @"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=risk;sslmode=none;";
        MySqlConnection con = new MySqlConnection(Connection);
        public ClsEmail()
        {

        }
        // for reset password from login screen to send link

        public void SendEmaillink(string Email, string subject, string body)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(emailFromAddress);
                    mail.To.Add(Email);
                    mail.Subject = "Reset Password Link";
                    mail.Body = $@"<html>
                    <body>
                    <p>Dear User,</p>
                    <p>{subject} </p>
                    
                    <p>Please Enter {body}.</p>


        <p>Please log in to the site using your credentials to review and take necessary action.</p>

        <p>
            <span>To Access Site: 
                <a href='{clienturl}' style='color: blue; text-decoration: none;'>Click Here</a>
            </span>
        </p>
                    <p>Best regards,<br/>
                     grcHAWK-Administrator
                    </p>
                </body>
              </html>";
                    mail.IsBodyHtml = true;
                    mail.Priority = MailPriority.High;
                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }
        }


        // Mail For Document Access In Document Awareness
        public void SendEmail(string emailToAddress, string subject, string body)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(emailFromAddress);
                    mail.To.Add(emailToAddress);
                    mail.Subject = "OTP To View Document Access Provided";
                    mail.Body = $@"<html>
                    <body>
                    <p>Dear User,</p>
                    <p>{subject} To View Documents</p>
                    
                    <p>{body}.</p>

        <p>Please log in to the site using your credentials to review and take necessary action.</p>

        <p>
            <span>To Access Site: 
                <a href='{clienturl}' style='color: blue; text-decoration: none;'>Click Here</a>
            </span>
        </p>
                    <p>Best regards,<br/>
                     grcHAWK-Administrator
                    </p>
                </body>
              </html>";
                    mail.IsBodyHtml = true;
                    mail.Priority = MailPriority.High;
                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }
        }
        // Mail for Schedule Assesss for taskuser
        public void Assessmentschedule(string emailToAddress, string templateName, int senderid, int userId, string baseUrl)
        {



            try
            {

                string Templatename = templateName;

                using (MailMessage mail = new MailMessage())
                {
                    int recciverid = userId;
                    int senderids = senderid;
                    mail.From = new MailAddress(emailFromAddress);
                    mail.To.Add(emailToAddress);
                    //var DocumentNames = string.Join(", ", documentNames);
                    //string documentListItems = string.Join("", documentNames.Select(doc => $"<li>{doc}</li>"));

                    mail.Subject = " Assessment Has Been Scheduled";
                    mail.Body = $@"<html>
    <body style='color: black; font-family: Arial, sans-serif;'>
        <p>Dear User,</p>
        <p>Assesement HAs been Scheduled ,Please Complete the Assessment :</p>
        
        {Templatename}
        
        <p>Please log in to the site using your credentials to view the details.</p>

        <p>
            <span>To Access Site: 
                <!-- Anchor tag with blue color -->
                <a href='{clienturl}' style='color: blue; text-decoration: none;'>
                    Click Here
                </a>
            </span>
        </p>

        <p>Best regards,<br/>
          grcHAWK-Administrator
        </p>
    </body>
</html>";


                    mail.IsBodyHtml = true;
                    mail.Priority = MailPriority.High;
                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                    }
                    // Open MySQL connection before executing the query
                    using (MySqlConnection con = new MySqlConnection(Connection))
                    {
                        con.Open();

                        string insertQuery = @"
                            INSERT INTO mailnotification 
                            (SenderID, ReceiverID, Subject, Body, SenderStatus, RecevierStatus, ThanksBody,created_at, updated_at) 
                            VALUES (@SenderID, @ReceiverID, @Subject, @Body, @SenderStatus, @RecevierStatus,@ThanksBody, NOW(), NOW())";

                        using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                        {
                            myCommand1.Parameters.AddWithValue("@SenderID", senderids);  // Set sender ID
                            myCommand1.Parameters.AddWithValue("@ReceiverID", recciverid); // Set receiver ID
                            myCommand1.Parameters.AddWithValue("@Subject", mail.Subject);
                            myCommand1.Parameters.AddWithValue("@Body", mail.Body);
                            myCommand1.Parameters.AddWithValue("@SenderStatus", "sent");  // Default to "sent"
                            myCommand1.Parameters.AddWithValue("@RecevierStatus", "delivered"); // Default to "sent"
                            myCommand1.Parameters.AddWithValue("@ThanksBody", "grcHAWK");
                            myCommand1.ExecuteNonQuery();
                        }
                    }


                }

            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }
        }

        // Mail For Assessment Provide Access  Template 

        public void AssessmentProvideAccessMail(string emailToAddress, string templateName, int senderid, int userId, string baseUrl)
        {



            try
            {

                string documentListItems = templateName;

                using (MailMessage mail = new MailMessage())
                {
                    int recciverid = userId;
                    int senderids = senderid;
                    mail.From = new MailAddress(emailFromAddress);
                    mail.To.Add(emailToAddress);
                    //var DocumentNames = string.Join(", ", documentNames);
                    //string documentListItems = string.Join("", documentNames.Select(doc => $"<li>{doc}</li>"));

                    mail.Subject = "Assessment Template Access Provided";
                    mail.Body = $@"<html>
    <body style='color: black; font-family: Arial, sans-serif;'>
        <p>Dear User,</p>
        <p> Access for the following Assessment Template(s) has been provided:</p>
        
        {documentListItems}
        
        <p>Please log in to the site using your credentials to view the details.</p>

        <p>
            <span>To Access Site: 
                <!-- Anchor tag with blue color -->
                <a href='{clienturl}' style='color: blue; text-decoration: none;'>
                    Click Here
                </a>
            </span>
        </p>

        <p>Best regards,<br/>
          grcHAWK-Administrator
        </p>
    </body>
</html>";


                    mail.IsBodyHtml = true;
                    mail.Priority = MailPriority.High;
                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                    }
                    // Open MySQL connection before executing the query
                    using (MySqlConnection con = new MySqlConnection(Connection))
                    {
                        con.Open();

                        string insertQuery = @"
                            INSERT INTO mailnotification 
                            (SenderID, ReceiverID, Subject, Body, SenderStatus, RecevierStatus, ThanksBody,created_at, updated_at) 
                            VALUES (@SenderID, @ReceiverID, @Subject, @Body, @SenderStatus, @RecevierStatus,@ThanksBody, NOW(), NOW())";

                        using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                        {
                            myCommand1.Parameters.AddWithValue("@SenderID", senderids);  // Set sender ID
                            myCommand1.Parameters.AddWithValue("@ReceiverID", recciverid); // Set receiver ID
                            myCommand1.Parameters.AddWithValue("@Subject", mail.Subject);
                            myCommand1.Parameters.AddWithValue("@Body", mail.Body);
                            myCommand1.Parameters.AddWithValue("@SenderStatus", "sent");  // Default to "sent"
                            myCommand1.Parameters.AddWithValue("@RecevierStatus", "delivered"); // Default to "sent"
                            myCommand1.Parameters.AddWithValue("@ThanksBody", "grcHAWK");
                            myCommand1.ExecuteNonQuery();
                        }
                    }


                }

            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }
        }

        // Mail For Provide Access to View Document
        public void ProvideAccessMail(string emailToAddress, string[] documentNames,int senderid, int userId,string baseUrl)
        {



            try
            {

                string documentListItems = string.Join("", documentNames.Select((doc) => $"<li>{doc}</li>"));

                using (MailMessage mail = new MailMessage())
                {
                    int recciverid = userId;
                    int senderids = senderid;
                    mail.From = new MailAddress(emailFromAddress);
                    mail.To.Add(emailToAddress);
                    //var DocumentNames = string.Join(", ", documentNames);
                    //string documentListItems = string.Join("", documentNames.Select(doc => $"<li>{doc}</li>"));

                    mail.Subject = "Document Access Provided";
                    mail.Body = $@"<html>
    <body style='color: black; font-family: Arial, sans-serif;'>
        <p>Dear User,</p>
        <p>Document Access for the following documents has been provided:</p>
        
        {documentListItems}
        
        <p>Please log in to the site using your credentials to view the details.</p>

        <p>
            <span>To Access Site: 
                <!-- Anchor tag with blue color -->
                <a href='{clienturl}' style='color: blue; text-decoration: none;'>
                    Click Here
                </a>
            </span>
        </p>

        <p>Best regards,<br/>
          grcHAWK-Administrator
        </p>
    </body>
</html>";


                    mail.IsBodyHtml = true;
                    mail.Priority = MailPriority.High;
                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                    }
                    // Open MySQL connection before executing the query
                    using (MySqlConnection con = new MySqlConnection(Connection))
                    {
                        con.Open();

                        string insertQuery = @"
                            INSERT INTO mailnotification 
                            (SenderID, ReceiverID, Subject, Body, SenderStatus, RecevierStatus, ThanksBody,created_at, updated_at) 
                            VALUES (@SenderID, @ReceiverID, @Subject, @Body, @SenderStatus, @RecevierStatus,@ThanksBody, NOW(), NOW())";

                        using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                        {
                            myCommand1.Parameters.AddWithValue("@SenderID", senderids );  // Set sender ID
                            myCommand1.Parameters.AddWithValue("@ReceiverID", recciverid ); // Set receiver ID
                            myCommand1.Parameters.AddWithValue("@Subject", mail.Subject);
                            myCommand1.Parameters.AddWithValue("@Body", mail.Body);
                            myCommand1.Parameters.AddWithValue("@SenderStatus", "sent");  // Default to "sent"
                            myCommand1.Parameters.AddWithValue("@RecevierStatus", "delivered"); // Default to "sent"
                            myCommand1.Parameters.AddWithValue("@ThanksBody", "grcHAWK");
                            myCommand1.ExecuteNonQuery();
                        }
                    }


                }

            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }
        }


        // Mail FOr Edit Access to View Document
        public void editAccessMail(string userEmail, string[] DocumentNames, int senderid, int userId, string baseUrl)
        {



            try
            {

                string documentListItems = string.Join("", DocumentNames.Select((doc) => $"<li>{doc}</li>"));

                using (MailMessage mail = new MailMessage())
                {
                    int recciverid = userId;
                    int senderids = senderid;
                    mail.From = new MailAddress(emailFromAddress);
                    mail.To.Add(userEmail);
                    //var DocumentNames = string.Join(", ", documentNames);
                    //string documentListItems = string.Join("", documentNames.Select(doc => $"<li>{doc}</li>"));

                    mail.Subject = "Document Access Provided";
                    mail.Body = $@"<html>
    <body style='color: black; font-family: Arial, sans-serif;'>
        <p>Dear User,</p>
        <p>Permissions  for the following documents has been provided:</p>
        
        {documentListItems}
        
        <p>Please log in to the site using your credentials to view the details.</p>

        <p>
            <span>To Access Site: 
                <!-- Anchor tag with blue color -->
                <a href='{clienturl}' style='color: blue; text-decoration: none;'>
                    Click Here
                </a>
            </span>
        </p>

        <p>Best regards,<br/>
          grcHAWK-Administrator
        </p>
    </body>
</html>";


                    mail.IsBodyHtml = true;
                    mail.Priority = MailPriority.High;
                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                    }
                    // Open MySQL connection before executing the query
                    using (MySqlConnection con = new MySqlConnection(Connection))
                    {
                        con.Open();

                        string insertQuery = @"
                            INSERT INTO mailnotification 
                            (SenderID, ReceiverID, Subject, Body, SenderStatus, RecevierStatus, ThanksBody,created_at, updated_at) 
                            VALUES (@SenderID, @ReceiverID, @Subject, @Body, @SenderStatus, @RecevierStatus,@ThanksBody, NOW(), NOW())";

                        using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                        {
                            myCommand1.Parameters.AddWithValue("@SenderID", senderids);  // Set sender ID
                            myCommand1.Parameters.AddWithValue("@ReceiverID", recciverid); // Set receiver ID
                            myCommand1.Parameters.AddWithValue("@Subject", mail.Subject);
                            myCommand1.Parameters.AddWithValue("@Body", mail.Body);
                            myCommand1.Parameters.AddWithValue("@SenderStatus", "sent");  // Default to "sent"
                            myCommand1.Parameters.AddWithValue("@RecevierStatus", "delivered"); // Default to "sent"
                            myCommand1.Parameters.AddWithValue("@ThanksBody", "grcHAWK");
                            myCommand1.ExecuteNonQuery();
                        }
                    }


                }

            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }
        }


        //Mail For Query/Issue Deatils
        public void NotifyReportingPerson(string reportingEmail, int reportingPersonId, int userid ,string subjectTitle ,string issueDetails, string baseUrl)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    int recciverid = reportingPersonId;
                    int senderids = userid;
                    mail.From = new MailAddress(emailFromAddress);
                    mail.To.Add(reportingEmail);

                    mail.Subject = "New Query Raised ";
                    mail.Body = $@"<html>
    <body style='color: black; font-family: Arial, sans-serif;'>
        <p>Dear Reporting Person,</p>
        <p>A new issue has been raised. Please check the details below:</p>

        <ul>
            <li><strong>Issue Title:</strong> {subjectTitle}</li>
            <li><strong>Description:</strong> {issueDetails}</li>
        </ul>

        <p>Please log in to the site using your credentials to review and take necessary action.</p>

        <p>
            <span>To Access Site: 
                <a href='{clienturl}' style='color: blue; text-decoration: none;'>Click Here</a>
            </span>
        </p>

        <p>Best regards,<br/>  grcHAWK-Administrator </p>
    </body>
</html>";

                    mail.IsBodyHtml = true;
                    mail.Priority = MailPriority.High;

                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                    }

                    // Save the email notification in the database
                    using (MySqlConnection con = new MySqlConnection(Connection))
                    {
                        con.Open();

                        string insertQuery = @"
                    INSERT INTO mailnotification 
                    (SenderID, ReceiverID, Subject, Body, SenderStatus, RecevierStatus, ThanksBody, created_at, updated_at) 
                    VALUES (@SenderID, @ReceiverID, @Subject, @Body, @SenderStatus, @RecevierStatus, @ThanksBody, NOW(), NOW())";

                        using (MySqlCommand myCommand = new MySqlCommand(insertQuery, con))
                        {
                            myCommand.Parameters.AddWithValue("@SenderID", senderids);
                            myCommand.Parameters.AddWithValue("@ReceiverID", recciverid);
                            myCommand.Parameters.AddWithValue("@Subject", mail.Subject);
                            myCommand.Parameters.AddWithValue("@Body", mail.Body);
                            myCommand.Parameters.AddWithValue("@SenderStatus", "sent");
                            myCommand.Parameters.AddWithValue("@RecevierStatus", "delivered");
                            myCommand.Parameters.AddWithValue("@ThanksBody", "grcHAWK");
                            myCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }


        // Mail For Tpa_user With Mapped Users List
        public void TpaMapedUsersUnLocked(string userEmail,  string description, string rqstname, int senderid, int userid)
        {
            try
            {
               // string User_names = string.Join("", firstNamesList.Select((users) => $"<li>{users}</li>"));

                using (MailMessage mail = new MailMessage())
                {
                    int recciverid = userid;
                    int senderids = senderid;
                    mail.From = new MailAddress(emailFromAddress);
                    mail.To.Add(userEmail);
                    //var DocumentNames = string.Join(", ", documentNames);
                    //string documentListItems = string.Join("", documentNames.Select(doc => $"<li>{doc}</li>"));

                    mail.Subject = " Request For External Assessment Scheduled";
                    mail.Body = $@"<html>
                <body>
                    <p>Dear User,</p>
                    <p>New Request For External Assessment Scheduled</p>
                     <p>Requesting Person: {rqstname}</p>
                    <p>Message : {description}</p>
                    
                        <p>Please log in to the site using your credentials to review and take necessary action.</p>

        <p>
            <span>To Access Site: 
                <a href='{clienturl}' style='color: blue; text-decoration: none;'>Click Here</a>
            </span>
        </p>
                        
                    
                    
                   <p>Thankyou and Regards</p>
                      grcHAWK-Administrator
                    </p>
                </body>
              </html>";
                    mail.IsBodyHtml = true;
                    mail.Priority = MailPriority.High;
                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                    }
                    using (MySqlConnection con = new MySqlConnection(Connection))
                    {
                        con.Open();

                        string insertQuery = @"
                    INSERT INTO mailnotification 
                    (SenderID, ReceiverID, Subject, Body, SenderStatus, RecevierStatus, ThanksBody, created_at, updated_at) 
                    VALUES (@SenderID, @ReceiverID, @Subject, @Body, @SenderStatus, @RecevierStatus, @ThanksBody, NOW(), NOW())";

                        using (MySqlCommand myCommand = new MySqlCommand(insertQuery, con))
                        {
                            myCommand.Parameters.AddWithValue("@SenderID", senderids);
                            myCommand.Parameters.AddWithValue("@ReceiverID", recciverid);
                            myCommand.Parameters.AddWithValue("@Subject", mail.Subject);
                            myCommand.Parameters.AddWithValue("@Body", mail.Body);
                            myCommand.Parameters.AddWithValue("@SenderStatus", "sent");
                            myCommand.Parameters.AddWithValue("@RecevierStatus", "delivered");
                            myCommand.Parameters.AddWithValue("@ThanksBody", "grcHAWK");
                            myCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }
        }


        public void TpaMapedUsersLocked(string userEmail, string userName)
        {
            try
            {
                ///  string User_names = string.Join("", firstNamesList.Select((users) => $"<li>{users}</li>"));

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(emailFromAddress);
                    mail.To.Add(userEmail);
                    //var DocumentNames = string.Join(", ", documentNames);
                    //string documentListItems = string.Join("", documentNames.Select(doc => $"<li>{doc}</li>"));

                    mail.Subject = "Mapped Users For Scheduled Assessment";
                    mail.Body = $@"<html>
                <body>
                    <p>Dear {userName},</p>
                    
                   
                     
                           <p>Please log in to the site using your credentials to review and take necessary action.</p>

        <p>
            <span>To Access Site: 
                <a href='{clienturl}' style='color: blue; text-decoration: none;'>Click Here</a>
            </span>
        </p>
                    <p>Best regards,<br/>
                      grcHAWK-Administrator
                    </p>
                </body>
              </html>";
                    mail.IsBodyHtml = true;
                    mail.Priority = MailPriority.High;
                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }


        }


            public void AssesmentProvideAccessMail(string emailToAddress, List<string> DocumentNames,string username)
            {
            try
            {
             //   var request = CurrentHttpContext.Request;
             //   string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);


                string documentListItems = string.Join("", DocumentNames.Select((doc) => $"<li>{doc}</li>"));

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(emailFromAddress);
                    mail.To.Add(emailToAddress);
                    //string host=HttpContext.Current.Request.Url.Host;
                    //string host = context.Request.Host.Host;
                    ////int port = context.Request.Host.Port ?;
                    //    string bodyLink = $"http://{host}:{port}";
                  //  string bodyLink = baseUrl;

                    //var DocumentNames = string.Join(", ", documentNames);
                    //string documentListItems = string.Join("", documentNames.Select(doc => $"<li>{doc}</li>"));

                    mail.Subject = "Assesment Document Access Provided";
                        mail.Body = $@"<html>
                <body>
                    <p>Dear {username},</p>
                    <p>We are pleased to inform you that access permissions for the following Assessment Templates have been granted:</p>
                    <p> Assessment Template Name :</p>
                        {documentListItems}
                    
                         <p>Please log in to the site using your credentials to review and take necessary action.</p>

        <p>
            <span>To Access Site: 
                <a href='{clienturl}' style='color: blue; text-decoration: none;'>Click Here</a>
            </span>
        </p>
                
                    <p>Best regards,<br/>
                      grcHAWK-Administrator
                    </p>
                </body>
              </html>";
                        mail.IsBodyHtml = true;
                        mail.Priority = MailPriority.High;
                        using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                        {
                            smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                            smtp.EnableSsl = enableSSL;
                            smtp.Send(mail);
                        }
                    }
                
            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }
            }

        // Mail for user Authentication
        public void SendAuthenticationEmail(string emailToAddress, string body)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(emailFromAddress);
                    mail.To.Add(emailToAddress);
                    mail.Subject = "OTP for Authentication";
                    mail.Body = $@"<html>
                <body>
                <p>Dear User,</p>
                <p>Please use the following OTP (One-Time Password) to validate your authentication for the site:</p>
                <p>Use: <strong>{body}</strong></p>
                <p><em>This OTP is valid for 5 minutes only.</em></p>


        <p>Please log in to the site using your credentials to review and take necessary action.</p>

        <p>
            <span>To Access Site: 
                <a href='{clienturl}' style='color: blue; text-decoration: none;'>Click Here</a>
            </span>
        </p>
                <p>Best regards,<br/> grcHAWK-Administrator</p>
              </body>
          </html>";
                    ;
                    mail.IsBodyHtml = true;
                    mail.Priority = MailPriority.High;
                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }
        }



        // Result Published
        public void Assessmentresultpublish(string emailToAddress, string templateName, int senderid, int userId, string baseUrl)
        {



            try
            {

                string documentListItems = templateName;

                using (MailMessage mail = new MailMessage())
                {
                    int recciverid = userId;
                    int senderids = senderid;
                    mail.From = new MailAddress(emailFromAddress);
                    mail.To.Add(emailToAddress);
                    //var DocumentNames = string.Join(", ", documentNames);
                    //string documentListItems = string.Join("", documentNames.Select(doc => $"<li>{doc}</li>"));

                    mail.Subject = "Assessment Result Published";
                    mail.Body = $@"<html>
    <body style='color: black; font-family: Arial, sans-serif;'>
        <p>Dear User,</p>
        <p> Result has been successfully Published for following Assessment:</p>
        
        {documentListItems}
        
        <p>Please log in to the site using your credentials to view the details.</p>

        <p>
            <span>To Access Site: 
                <!-- Anchor tag with blue color -->
                <a href='{clienturl}' style='color: blue; text-decoration: none;'>
                    Click Here
                </a>
            </span>
        </p>

        <p>Best regards,<br/>
          grcHAWK-Administrator
        </p>
    </body>
</html>";


                    mail.IsBodyHtml = true;
                    mail.Priority = MailPriority.High;
                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                    }
                    // Open MySQL connection before executing the query
                    using (MySqlConnection con = new MySqlConnection(Connection))
                    {
                        con.Open();

                        string insertQuery = @"
                            INSERT INTO mailnotification 
                            (SenderID, ReceiverID, Subject, Body, SenderStatus, RecevierStatus, ThanksBody,created_at, updated_at) 
                            VALUES (@SenderID, @ReceiverID, @Subject, @Body, @SenderStatus, @RecevierStatus,@ThanksBody, NOW(), NOW())";

                        using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                        {
                            myCommand1.Parameters.AddWithValue("@SenderID", senderids);  // Set sender ID
                            myCommand1.Parameters.AddWithValue("@ReceiverID", recciverid); // Set receiver ID
                            myCommand1.Parameters.AddWithValue("@Subject", mail.Subject);
                            myCommand1.Parameters.AddWithValue("@Body", mail.Body);
                            myCommand1.Parameters.AddWithValue("@SenderStatus", "sent");  // Default to "sent"
                            myCommand1.Parameters.AddWithValue("@RecevierStatus", "delivered"); // Default to "sent"
                            myCommand1.Parameters.AddWithValue("@ThanksBody", "grcHAWK");
                            myCommand1.ExecuteNonQuery();
                        }
                    }


                }

            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }
        }



        //mitigation Suggestion to Process Owner

        public void suggestiontoprocessowner(string emailToAddress, string templateName, int senderid, int userId, string baseUrl)
        {



            try
            {

                string documentListItems = templateName;

                using (MailMessage mail = new MailMessage())
                {
                    int recciverid = userId;
                    int senderids = senderid;
                    mail.From = new MailAddress(emailFromAddress);
                    mail.To.Add(emailToAddress);
                    //var DocumentNames = string.Join(", ", documentNames);
                    //string documentListItems = string.Join("", documentNames.Select(doc => $"<li>{doc}</li>"));

                    mail.Subject = " Mitigation Task Update for Assessment";
                    mail.Body = $@"<html>
    <body style='color: black; font-family: Arial, sans-serif;'>
        <p>Dear User,</p>
        <p> You have been assigned responsibility for Mitigation Task Action  related to  following Assessment:</p>
        
        {documentListItems}
        
        <p>Please log in to the site using your credentials to view the details.</p>

        <p>
            <span>To Access Site: 
                <!-- Anchor tag with blue color -->
                <a href='{clienturl}' style='color: blue; text-decoration: none;'>
                    Click Here
                </a>
            </span>
        </p>

        <p>Best regards,<br/>
          grcHAWK-Administrator
        </p>
    </body>
</html>";


                    mail.IsBodyHtml = true;
                    mail.Priority = MailPriority.High;
                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                    }
                    // Open MySQL connection before executing the query
                    using (MySqlConnection con = new MySqlConnection(Connection))
                    {
                        con.Open();

                        string insertQuery = @"
                            INSERT INTO mailnotification 
                            (SenderID, ReceiverID, Subject, Body, SenderStatus, RecevierStatus, ThanksBody,created_at, updated_at) 
                            VALUES (@SenderID, @ReceiverID, @Subject, @Body, @SenderStatus, @RecevierStatus,@ThanksBody, NOW(), NOW())";

                        using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                        {
                            myCommand1.Parameters.AddWithValue("@SenderID", senderids);  // Set sender ID
                            myCommand1.Parameters.AddWithValue("@ReceiverID", recciverid); // Set receiver ID
                            myCommand1.Parameters.AddWithValue("@Subject", mail.Subject);
                            myCommand1.Parameters.AddWithValue("@Body", mail.Body);
                            myCommand1.Parameters.AddWithValue("@SenderStatus", "sent");  // Default to "sent"
                            myCommand1.Parameters.AddWithValue("@RecevierStatus", "delivered"); // Default to "sent"
                            myCommand1.Parameters.AddWithValue("@ThanksBody", "grcHAWK");
                            myCommand1.ExecuteNonQuery();
                        }
                    }


                }

            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }
        }
    }
    }
