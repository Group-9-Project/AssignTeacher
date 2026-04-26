INSERT INTO AppUsers (FullName, Email, PasswordHash, Role, CreatedAt, IsBlocked)
SELECT 
    FullName, 
    ParentEmail, 
    Password, 
    1, -- 1 = Student Role
    GETDATE(), 
    0
FROM Student
WHERE ParentEmail NOT IN (SELECT Email FROM AppUsers);