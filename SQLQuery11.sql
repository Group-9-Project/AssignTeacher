SELECT * FROM AppUsers u
LEFT JOIN StudentAccount s ON u.Id = s.Id
WHERE u.Role = 1 AND s.Id IS NULL