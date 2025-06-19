using DomainModel;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITRTelemetry.IOtpService
{
    public interface IOtpService
    {
        Task<DateTime> SaveOtpAsync(int userId, string otp);
        Task<bool> ValidateOtpAsync(int userId, string otp);
        Task<bool> ValidateUserAsync(string username, string password);
        Task<string> GenerateOtpAsync(string username);
        Task<bool> VerifyOtpAsync(string username, string otp);
        string GenerateJwtToken(string username);
    }
   

}
