SELECT * FROM Student
WHERE StudentNumber = '2026STU007';
INSERT INTO AppUsers (FullName, Email, PasswordHash, Role, CreatedAt, IsBlocked)
VALUES (
    'Azande Khoza', 
    'minenhlexulu061@gmail.com', 
    '2ebe42f7', -- Using the existing password hash from your screenshot
    1,          -- Role 1 for Student
    GETDATE(), 
    0
);
SELECT Id, Email FROM AppUsers WHERE Email = 'minenhlexulu061@gmail.com';
SELECT 
    s.FullName AS StudentName, 
    u.FullName AS AppUserName,
    u.Id AS NewAppUserId
FROM Student s
JOIN AppUsers u ON s.ParentEmail = u.Email
WHERE s.StudentNumber = '2026STU007';

-- Ensure the 'AppUsers' table has a record for the student email
IF NOT EXISTS (SELECT 1 FROM AppUsers WHERE Email = 'minenhlexulu061@gmail.com')
BEGIN
    INSERT INTO AppUsers (FullName, Email, PasswordHash, Role, CreatedAt, IsBlocked)
    VALUES ('Azande Khoza', 'minenhlexulu061@gmail.com', '2ebe42f7', 1, GETDATE(), 0);
END