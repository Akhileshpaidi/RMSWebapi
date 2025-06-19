
using System.Collections.Generic;
using System.Security.Claims; 
using System.Text;
using System.Threading.Tasks;
using System;
using ITRTelemetry.IOtpService;
using MySQLProvider;
using DomainModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using ITR_TelementaryAPI.Controllers;
using Microsoft.AspNetCore.Mvc;

public class OtpService : IOtpService
{
        private readonly string _connectionString;

        public OtpService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("myDb1");
        }

    public string GenerateJwtToken(string username)
    {
        throw new NotImplementedException();
    }

    public Task<string> GenerateOtpAsync(string username)
    {
        throw new NotImplementedException();
    }


    public async Task<DateTime> SaveOtpAsync(int userId, string otp)
    {
        using var con = new MySqlConnection(_connectionString);
        await con.OpenAsync();

        string checkQuery = "SELECT COUNT(*) FROM UserOtps WHERE userId = @userId";
        using var checkCmd = new MySqlCommand(checkQuery, con);
        checkCmd.Parameters.AddWithValue("@userId", userId);

        var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;

        string query = exists
            ? "UPDATE UserOtps SET otp = @otp, expiryTime = @expiryTime WHERE userId = @userId"
            : "INSERT INTO UserOtps (userId, otp, expiryTime) VALUES (@userId, @otp, @expiryTime)";

        DateTime expiryTime = DateTime.Now.AddMinutes(5); 

        using var cmd = new MySqlCommand(query, con);
        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@otp", otp);
        cmd.Parameters.AddWithValue("@expiryTime", expiryTime);

        await cmd.ExecuteNonQueryAsync();

        return expiryTime;
    }


    public async Task<bool> ValidateOtpAsync(int userId, string otp)
    {
        using var con = new MySqlConnection(_connectionString);
        await con.OpenAsync();

        string selectQuery = @"SELECT 1 FROM UserOtps 
                           WHERE userId = @userId AND otp =@otp AND expiryTime > @now
                           LIMIT 1";

        using var cmd = new MySqlCommand(selectQuery, con);
        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@otp", otp);
        cmd.Parameters.AddWithValue("@now", DateTime.Now);

        var isValid = await cmd.ExecuteScalarAsync() != null;

        // Optional: delete the OTP after use to prevent reuse
        if (isValid)
        {
            string deleteQuery = "DELETE FROM UserOtps WHERE UserId = @userId";
            using var deleteCmd = new MySqlCommand(deleteQuery, con);
            deleteCmd.Parameters.AddWithValue("@userId", userId);
            await deleteCmd.ExecuteNonQueryAsync();
        }

        return isValid;
    }


    public Task<bool> ValidateUserAsync(string username, string password)
    {
        throw new NotImplementedException();
    }

    public Task<bool> VerifyOtpAsync(string username, string otp)
    {
        throw new NotImplementedException();
    }


}
