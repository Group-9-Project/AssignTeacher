using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SchoolTimetable.Models;

namespace SchoolTimetable.Services
{
    public class EmailService
    {
        // Hardcoded Gmail Credentials
        private readonly string _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _username = "durbangirlssecondarys@gmail.com";
        private readonly string _password = "gepx sjbq twda lxnq"; // App Password
        private readonly string _fromAddress = "durbangirlssecondarys@gmail.com";
        private readonly string _fromName = "Durban Girls Secondary School";

        
        public bool SendRegistrationFeeConfirmation(string parentName, string studentName, string email, string referenceNum)
        {
            var body = $@"
            <html>
            <body style='font-family:Arial,sans-serif; background-color:#f4f7f6; padding:20px;'>
                <div style='max-width:600px; margin:0 auto; background:#fff; border-radius:12px; overflow:hidden; border:1px solid #eee; box-shadow:0 4px 12px rgba(0,0,0,0.1);'>
                    <div style='background:#10b981; padding:25px; text-align:center;'>
                        <h2 style='color:#ffffff; margin:0;'>Payment Received</h2>
                        <p style='color:rgba(255,255,255,0.8); margin:5px 0 0;'>Durban Girls Secondary School</p>
                    </div>
                    <div style='padding:30px; line-height:1.6; color:#333;'>
                        <p>Dear <strong>{parentName}</strong>,</p>
                        <p>This email confirms that we have successfully received the <strong>R1000.00</strong> registration fee for <strong>{studentName}</strong>.</p>
                        
                        <div style='background:#f8fafc; border:1px solid #e2e8f0; padding:15px; border-radius:8px; margin:20px 0;'>
                            <p style='margin:5px 0;'><strong>Status:</strong> <span style='color:#10b981; font-weight:bold;'>Fully Paid</span></p>
                            <p style='margin:5px 0;'><strong>Reference:</strong> {referenceNum}</p>
                            <p style='margin:5px 0;'><strong>Date:</strong> {DateTime.Now:dd MMMM yyyy}</p>
                        </div>

                        <p>The student's placement is now finalized. You can continue to use the parent portal to track academic progress and school notices.</p>

                        <hr style='border:0; border-top:1px solid #eee; margin:25px 0;' />
                        <p style='font-size:12px; color:#999; text-align:center;'>&copy; {DateTime.Now.Year} Durban Girls Secondary School Finance</p>
                    </div>
                </div>
            </body>
            </html>";

            return SendEmail(email, "Registration Fee Received - " + studentName, body);
        }

        // --- NEW METHODS FOR APPLICATIONS (RETAINED) ---

        public bool SendApplicationAcceptance(Application app, string studentNum, string tempPass)
        {
            var body = $@"
            <html>
            <body style='font-family:Arial,sans-serif; background-color:#f4f7f6; padding:20px;'>
                <div style='max-width:600px; margin:0 auto; background:#fff; border-radius:12px; overflow:hidden; border:1px solid #eee; box-shadow:0 4px 12px rgba(0,0,0,0.1);'>
                    <div style='background:#10b981; padding:25px; text-align:center;'>
                        <h2 style='color:#ffffff; margin:0;'>Application Accepted!</h2>
                        <p style='color:rgba(255,255,255,0.8); margin:5px 0 0;'>Durban Girls Secondary School</p>
                    </div>
                    <div style='padding:30px; line-height:1.6; color:#333;'>
                        <p>Dear Parent,</p>
                        <p>We are pleased to inform you that the application for <strong>{app.FirstName} {app.LastName}</strong> has been successful.</p>
                        <div style='background:#f0fdf4; border-left:4px solid #10b981; padding:15px; margin:20px 0;'>
                            <h4 style='margin-top:0; color:#065f46;'>Portal Access Credentials</h4>
                            <p style='margin:5px 0;'><strong>Student Number:</strong> {studentNum}</p>
                            <p style='margin:5px 0;'><strong>Temporary Password:</strong> <span style='font-weight:bold;'>{tempPass}</span></p>
                        </div>
                        <p style='background:#fff9c4; padding:10px; border-radius:4px; border:1px solid #fbc02d; font-size:14px;'>
                            <strong>Next Step:</strong> Please log in to the portal and pay the <strong>R1000 registration fee</strong> to finalize the admission.
                        </p>
                        <hr style='border:0; border-top:1px solid #eee; margin:25px 0;' />
                        <p style='font-size:12px; color:#999; text-align:center;'>&copy; {DateTime.Now.Year} Durban Girls Secondary School Admissions</p>
                    </div>
                </div>
            </body>
            </html>";
            return SendEmail(app.Email, "Application Accepted - Durban Girls Secondary", body);
        }

        public bool SendApplicationRejection(Application app, string feedback)
        {
            var body = $@"
            <html>
            <body style='font-family:Arial,sans-serif; background-color:#f4f7f6; padding:20px;'>
                <div style='max-width:600px; margin:0 auto; background:#fff; border-radius:12px; overflow:hidden; border:1px solid #eee; box-shadow:0 4px 12px rgba(0,0,0,0.1);'>
                    <div style='background:#ef4444; padding:25px; text-align:center;'>
                        <h2 style='color:#ffffff; margin:0;'>Application Update</h2>
                    </div>
                    <div style='padding:30px; line-height:1.6; color:#333;'>
                        <p>Dear Parent,</p>
                        <p>Regarding the application for <strong>{app.FirstName} {app.LastName}</strong>, we regret to inform you that we are unable to offer a placement at this time.</p>
                        <div style='background:#fef2f2; border-left:4px solid #ef4444; padding:15px; margin:20px 0;'>
                            <h4 style='margin-top:0; color:#991b1b;'>Administrator Feedback</h4>
                            <p style='margin:5px 0; font-style:italic;'>""{(string.IsNullOrWhiteSpace(feedback) ? "The application does not meet our current entry requirements." : feedback)}""</p>
                        </div>
                        <hr style='border:0; border-top:1px solid #eee; margin:25px 0;' />
                        <p style='font-size:12px; color:#999; text-align:center;'>&copy; {DateTime.Now.Year} Durban Girls Secondary School Admissions</p>
                    </div>
                </div>
            </body>
            </html>";
            return SendEmail(app.Email, "Application Update - Durban Girls Secondary", body);
        }

        // --- EXISTING METHODS (RETAINED) ---

        public bool SendParentResetCode(string parentName, string email, string otpCode)
        {
            var body = $@"
            <html>
            <body style='font-family:Arial,sans-serif; background-color:#f4f7f6; padding:20px;'>
                <div style='max-width:500px; margin:0 auto; background:#fff; border-radius:12px; border:1px solid #eee;'>
                    <div style='background:#1a1a2e; padding:25px; text-align:center;'>
                        <h2 style='color:#ffffff; margin:0;'>Password Reset</h2>
                    </div>
                    <div style='padding:30px; line-height:1.6; color:#333;'>
                        <p>Dear <strong>{parentName}</strong>,</p>
                        <p>Please use the verification code below to proceed:</p>
                        <div style='background:#f3f0ff; border:2px dashed #7c6af7; margin:25px 0; padding:20px; text-align:center;'>
                            <span style='font-size:32px; font-weight:800; letter-spacing:8px; color:#7c6af7;'>{otpCode}</span>
                        </div>
                    </div>
                </div>
            </body>
            </html>";
            return SendEmail(email, "Your Parent Account Reset Code", body);
        }

        public bool SendEntranceTestInvite(string email, string studentName, string testTitle, string date, string time, string duration)
        {
            var body = $@"
    <html>
    <body style='font-family:Arial,sans-serif; background-color:#f4f7f6; padding:20px;'>
        <div style='max-width:600px; margin:0 auto; background:#fff; border-radius:12px; overflow:hidden; border:1px solid #eee; box-shadow:0 4px 12px rgba(0,0,0,0.1);'>
            <div style='background:#6b4bff; padding:25px; text-align:center;'>
                <h2 style='color:#ffffff; margin:0;'>Entrance Test Scheduled</h2>
                <p style='color:rgba(255,255,255,0.8); margin:5px 0 0;'>Durban Girls Secondary School Admissions</p>
            </div>
            <div style='padding:30px; line-height:1.6; color:#333;'>
                <p>Dear Parent,</p>
                <p>We are pleased to invite <strong>{studentName}</strong> to sit for the upcoming entrance examination. This follows the successful confirmation of your registration fee.</p>
                
                <div style='background:#f3f0ff; border:1px solid #dcd7ff; padding:20px; border-radius:8px; margin:20px 0;'>
                    <h4 style='margin-top:0; color:#6b4bff; border-bottom:1px solid #dcd7ff; padding-bottom:10px;'>Test Details</h4>
                    <table style='width:100%; font-size:14px;'>
                        <tr>
                            <td style='padding:5px 0; color:#666;'><strong>Examination:</strong></td>
                            <td style='padding:5px 0;'>{testTitle}</td>
                        </tr>
                        <tr>
                            <td style='padding:5px 0; color:#666;'><strong>Date:</strong></td>
                            <td style='padding:5px 0;'>{date}</td>
                        </tr>
                        <tr>
                            <td style='padding:5px 0; color:#666;'><strong>Start Time:</strong></td>
                            <td style='padding:5px 0;'>{time}</td>
                        </tr>
                        <tr>
                            <td style='padding:5px 0; color:#666;'><strong>Duration:</strong></td>
                            <td style='padding:5px 0;'>{duration} Minutes</td>
                        </tr>
                    </table>
                </div>

                <div style='background:#fff9c4; padding:15px; border-radius:4px; border:1px solid #fbc02d; font-size:14px; color:#856404;'>
                    <strong>Instructions:</strong> Please ensure the student is logged into their portal account at least 10 minutes before the scheduled start time. A stable internet connection is required.
                </div>

                <p style='margin-top:20px;'>If you have any queries regarding this schedule, please contact the admissions office immediately.</p>

                <hr style='border:0; border-top:1px solid #eee; margin:25px 0;' />
                <p style='font-size:12px; color:#999; text-align:center;'>&copy; {DateTime.Now.Year} Durban Girls Secondary School Admissions Team</p>
            </div>
        </div>
    </body>
    </html>";

            return SendEmail(email, "Entrance Test Invitation - " + studentName, body);
        }

        public bool SendTimetableEmail(Teacher teacher, List<TimetableSlot> slots, string email, string tempPassword)
        {
            var days = new[] { 1, 2, 3, 4, 5 };
            var dayNames = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
            var tableRows = "";

            for (int p = 1; p <= 8; p++)
            {
                var periodTime = GetPeriodTime(p);
                tableRows += $@"<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold;background:#f9f9f9;'>Period {p}<br/><small>{periodTime}</small></td>";
                foreach (var day in days)
                {
                    var slot = slots.Find(s => s.DayOfWeek == day && s.Period == p);
                    if (slot != null)
                        tableRows += $@"<td style='padding:8px;border:1px solid #ddd;background:#fff;'><strong>{slot.Subject.Code}</strong><br/>{slot.Class.Name}</td>";
                    else
                        tableRows += "<td style='padding:8px;border:1px solid #ddd;text-align:center;color:#ccc;'>-</td>";
                }
                tableRows += "</tr>";
            }

            var headerCells = string.Join("", dayNames.Select(d => $"<th style='padding:12px;border:1px solid #444;'>{d}</th>"));

            // --- NEW CREDENTIALS SECTION ---
            var credentialsSection = $@"
        <div style='background:#f4f4f9; border-left:4px solid #1a1a2e; padding:15px; margin-bottom:25px;'>
            <h3 style='margin-top:0; color:#1a1a2e;'>Staff Portal Access</h3>
            <p style='margin:5px 0;'><strong>Username:</strong> {email}</p>
            <p style='margin:5px 0;'><strong>Temporary Password:</strong> <span style='color:#d4af37; font-weight:bold;'>{tempPassword}</span></p>
            <p style='font-size:0.85rem; color:#666;'>Please use these credentials to log in and update your profile.</p>
        </div>";

            var body = $@"
        <html>
        <body style='font-family:Arial, sans-serif; color:#333;'>
            <div style='background:#1a1a2e; padding:30px; text-align:center'>
                <h1 style='color:#fff; margin:0;'>Durban Girls Secondary</h1>
                <p style='color:#d4af37; margin:5px 0;'>Weekly Timetable & Account Info</p>
            </div>
            <div style='padding:24px;'>
                <p>Dear <strong>{teacher.FullName}</strong>,</p>
                
                {credentialsSection}

                <h3 style='color:#1a1a2e;'>Your Schedule</h3>
                <table style='width:100%; border-collapse:collapse;'>
                    <thead>
                        <tr style='background:#1a1a2e; color:#fff'>
                            <th style='padding:12px; border:1px solid #444;'>Period</th>
                            {headerCells}
                        </tr>
                    </thead>
                    <tbody>
                        {tableRows}
                    </tbody>
                </table>
            </div>
            <div style='text-align:center; padding:20px; font-size:0.8rem; color:#888;'>
                © {DateTime.Now.Year} Durban Girls Secondary School Admin System
            </div>
        </body>
        </html>";

            return SendEmail(email, "Your Weekly Timetable & Login Credentials - DGSS", body);
        }

        public bool SendSubstitutionEmail(Teacher teacher, Substitution sub, bool isSubstitute)
        {
            var slot = sub.TimetableSlot;
            var body = $@"<html><body><h2 style='color:{(isSubstitute ? "#e67e22" : "#2c3e50")};'>Substitution Notice</h2><p>Dear <strong>{teacher.FullName}</strong>,</p><p><strong>Date:</strong> {sub.SubstitutionDate:dddd, dd MMMM yyyy}</p><p><strong>Subject:</strong> {slot?.Subject?.Name}</p></body></html>";
            return SendEmail(teacher.Email, "Substitution Notice - DGSS", body);
        }

        public bool SendEmail(string to, string subject, string htmlBody)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_fromName, _fromAddress));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;
                message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();
                using (var client = new SmtpClient())
                {
                    client.Connect(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
                    client.Authenticate(_username, _password);
                    client.Send(message);
                    client.Disconnect(true);
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Email error: " + ex.Message);
                return false;
            }
        }

        private string GetPeriodTime(int period)
        {
            switch (period)
            {
                case 1: return "07:30-08:15";
                case 2: return "08:20-09:05";
                case 3: return "09:10-09:55";
                case 4: return "10:15-11:00";
                case 5: return "11:05-11:50";
                case 6: return "12:10-12:55";
                case 7: return "13:00-13:45";
                case 8: return "13:50-14:30";
                default: return "-";
            }
        }
    }
}