using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApp.Application.DTOs.User
{
    /// <summary>
    /// DTO for updating user password
    /// </summary>
    public class UpdatePasswordUserDto
    {
        /// <summary>
        /// Current password for verification
        /// </summary>
        public string CurrentPassword { get; set; } = string.Empty;
        
        /// <summary>
        /// New password to set
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;
    }
}
