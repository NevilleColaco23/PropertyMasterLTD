using System;

namespace EmailWorker.Tests
{
    /// <summary>
    /// Quick test to verify HTML email template generation
    /// Run this to see the actual HTML that will be sent
    /// </summary>
    class EmailTemplateTest
    {
        static void Main(string[] args)
        {
            var username = "testuser";
            var userId = 123;
            var token = "abc123xyz789";
            var activationLink = $"https://property-master-silk.vercel.app/activate?userId={userId}&token={token}";
            
            var htmlBody = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Activate Your Account</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
        <tr>
            <td align=""center"" style=""padding: 40px 0;"">
                <table role=""presentation"" style=""width: 600px; border-collapse: collapse; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <h1 style=""color: #333333; margin: 0 0 20px 0; font-size: 24px; border-bottom: 3px solid #4CAF50; padding-bottom: 15px;"">
                                Welcome to Property Master!
                            </h1>
                            <p style=""color: #555555; font-size: 16px; line-height: 1.6; margin: 20px 0;"">
                                Hi <strong>{username}</strong>,
                            </p>
                            <p style=""color: #555555; font-size: 14px; line-height: 1.6; margin: 20px 0;"">
                                Thank you for signing up for Property Master. To complete your registration, please activate your account by clicking the button below:
                            </p>
                            <table role=""presentation"" style=""margin: 30px auto;"">
                                <tr>
                                    <td align=""center"" style=""border-radius: 5px; background-color: #4CAF50;"">
                                        <a href=""{activationLink}"" target=""_blank"" style=""display: inline-block; padding: 15px 30px; font-size: 16px; color: #ffffff; text-decoration: none; border-radius: 5px; font-weight: bold;"">
                                            Activate Account
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            <p style=""color: #888888; font-size: 12px; line-height: 1.6; margin: 20px 0;"">
                                Or copy and paste this link into your browser:
                            </p>
                            <p style=""color: #4CAF50; font-size: 12px; word-break: break-all; background-color: #f9f9f9; padding: 10px; border-radius: 4px;"">
                                {activationLink}
                            </p>
                            <hr style=""border: none; border-top: 1px solid #eeeeee; margin: 30px 0;"">
                            <p style=""color: #888888; font-size: 12px; line-height: 1.6; margin: 10px 0;"">
                                <strong>Note:</strong> This activation link will expire in 24 hours.
                            </p>
                            <p style=""color: #888888; font-size: 12px; line-height: 1.6; margin: 10px 0;"">
                                If you did not sign up for this account, please ignore this email.
                            </p>
                            <p style=""color: #555555; font-size: 14px; line-height: 1.6; margin: 30px 0 0 0;"">
                                Best regards,<br>
                                <strong>Property Master Team</strong>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

            Console.WriteLine("=== EMAIL HTML OUTPUT ===");
            Console.WriteLine(htmlBody);
            Console.WriteLine("\n=== EMAIL PREVIEW ===");
            Console.WriteLine($"To: testuser@example.com");
            Console.WriteLine($"Subject: Activate Your Account");
            Console.WriteLine($"From: Property Master <noreply@masterproperty.site>");
            Console.WriteLine("\nHTML Length: " + htmlBody.Length + " characters");
            
            // Save to file for browser preview
            System.IO.File.WriteAllText("email_preview.html", htmlBody);
            Console.WriteLine("\n✅ HTML saved to: email_preview.html");
            Console.WriteLine("📧 Open this file in your browser to see how it looks!");
        }
    }
}
