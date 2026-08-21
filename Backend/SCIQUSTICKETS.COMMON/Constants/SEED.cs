namespace SCIQUSTICKETS.COMMON.Constants
{
    /// <summary>
    /// Static seed data constants used across all layers.
    /// These values are fixed and used for EF Core HasData() seeding.
    /// </summary>
    public static class SEED
    {
        // ── Seed Date ──────────────────────────────────────────────────
        public static readonly DateTime SeedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // ── Roles ──────────────────────────────────────────────────────
        public static readonly string AdminRoleId = "A1B2C3D4-E5F6-7890-ABCD-EF1234567890";
        public static readonly string AdminRoleConcurrencyStamp = "F7A3C2B1-D4E5-6789-ABCD-EF0123456789";
        public static readonly string AdminRole = "Admin";

        // ── Admin User ─────────────────────────────────────────────────
        public static readonly string AdminUserId = "1022da6f-76cb-45ae-b0de-6c663373c4bf";
		public const string SystemActorUserId = "c064137c-2a06-4c13-bbd7-52f9c5d81722";
		public static readonly string AdminEmailId = "admin@sciqustickets.com";
        public static readonly string AdminPassword = "Admin@123";
        public static readonly string AdminPasswordHash = "AQAAAAIAAYagAAAAEIdzJ5KmybApslqWPl/Ax9qNNMhb6GZ7dUoH/WMdLGZRGd2J5437zxPGdYU9FIvpDQ==";
        public static readonly string AdminSecurityStamp = "C64E8E81-807D-4074-9C78-A6305F8F7504";
        public static readonly string AdminConcurrencyStamp = "B3D2F62C-0B4A-4A7F-A5C1-E8A93B7D034C";
        public static readonly string AdminName = "Super Admin";
        public static readonly string AdminRegisteredMobileNumber = "9999999999";
        public static readonly string? AdminSecondMobileNumber = null;
        public static readonly string AdminEmployeeId = "EMP-0001";
        public static readonly string? AdminProfileImageUrl = null;

        // ── Employee / Staff Format ────────────────────────────────────
        public static readonly string EmployeeIdFormat = "EMP-";

        // ── Department (placeholder — fill in once Dept team seeds theirs) ──
        public static readonly string DeptId = "D1E2F3A4-B5C6-7890-ABCD-123456789000";
    }
}
