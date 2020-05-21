DROP FUNCTION IF EXISTS scanExists;

DELIMITER $
CREATE FUNCTION scanExists(_id INT) RETURNS BOOLEAN
BEGIN
    RETURN (SELECT COUNT(*) FROM scans WHERE scans.id = _id) = 1;
END $ DELIMITER ;
