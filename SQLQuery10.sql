SELECT
    sa.Id AS StudentAccountId,
    sa.StudentNumber,
    sa.TemporaryPassword,
    sa.IsActive,
    au.Id AS AppUserId
FROM StudentAccount sa
LEFT JOIN AppUsers au ON sa.Id = au.Id;


INSERT INTO AppUsers (Id, FullName, Email, PasswordHash, Role, CreatedAt)
SELECT
    sa.Id,
    sa.FirstName + ' ' + sa.LastName,
    sa.Email,
    'TEMP',        -- not used for student login
    1,             -- UserRole.Student
    GETDATE()
FROM StudentAccount sa
LEFT JOIN AppUsers au ON sa.Id = au.Id
WHERE au.Id IS NULL;

SET IDENTITY_INSERT AppUsers ON;

INSERT INTO AppUsers (Id, FullName, Email, PasswordHash, Role, CreatedAt)
SELECT
    sa.Id,
    sa.FirstName + ' ' + sa.LastName,
    sa.Email,
    'TEMP',        -- placeholder (students do NOT log in via AppUser)
    1,             -- Student role
    GETDATE()
FROM StudentAccount sa
LEFT JOIN AppUsers au ON sa.Id = au.Id
WHERE au.Id IS NULL;

SET IDENTITY_INSERT AppUsers OFF;


SELECT
    sa.Id AS StudentAccountId,
    au.Id AS AppUserId
FROM StudentAccount sa
JOIN AppUsers au ON sa.Id = au.I

SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'StudentAccount';

SET IDENTITY_INSERT AppUsers ON;

INSERT INTO AppUsers (
    Id,
    FullName,
    Email,
    PasswordHash,
    Role,
    CreatedAt
)
SELECT
    sa.Id,
    sa.FirstName + ' ' + sa.LastName,
    sa.Email,
    'TEMP',
    1,
    GETDATE()
FROM StudentAccount sa
LEFT JOIN AppUsers au ON au.Id = sa.Id
WHERE au.Id IS NULL;

SET IDENTITY_INSERT AppUsers OFF;

SELECT
    sa.Id AS StudentAccountId,
    au.Id AS AppUserId
FROM StudentAccount sa
JOIN AppUsers au ON sa.Id = au.Id;

SELECT * FROM AppUsers;
SELECT * FROM StudentAccount;

SELECT sa.Id, au.Id
FROM StudentAccount sa
JOIN AppUsers au ON sa.Id = au.Id;

