DROP FUNCTION IF EXISTS assetExists;

DELIMITER $ CREATE FUNCTION assetExists(_id INT) RETURNS BOOLEAN
BEGIN
  RETURN (SELECT COUNT(*) FROM assets WHERE assets.id = _id) = 1;
END $ DELIMITER ;
DROP FUNCTION IF EXISTS getRoomIdWithAsset;

/*  If the asset is not allocated function returns NULL */

DELIMITER $ CREATE FUNCTION getRoomIdWithAsset(id_asset INT) RETURNS INT
BEGIN
  DECLARE Room_id INT DEFAULT NULL;
  DECLARE Deleted BOOLEAN DEFAULT TRUE;

  SELECT
    reports.room,
    NOT reports_positions.present 
  INTO
    Room_id,
    Deleted
  FROM
    reports_positions
    JOIN reports ON reports_positions.report_id = reports.id
  WHERE
    reports_positions.asset_id = id_asset
    AND NOT (
      reports_positions.previous_room != reports.room
      AND NOT reports_positions.present
    ) /* skip 'do nothing' positions */
  ORDER BY
    reports.create_date DESC,
    reports.id DESC
  LIMIT
    1;

  IF Deleted THEN
    SET Room_id = NULL;
  END IF;

  RETURN Room_id;
END $ DELIMITER ;
DROP FUNCTION IF EXISTS haveDublicates;

DELIMITER $ CREATE FUNCTION haveDublicates(table_name VARCHAR(32), ids VARCHAR(1024), have_dublicates BOOLEAN) RETURNS VARCHAR(64)
BEGIN
  RETURN IF(have_dublicates, NULL, CONCAT(table_name, " id=", ids, " have duplicates"));
END $ DELIMITER ;
DROP FUNCTION IF EXISTS idsNotFound;

DELIMITER $ CREATE FUNCTION idsNotFound(table_name VARCHAR(32), ids VARCHAR(1024), is_found BOOLEAN) RETURNS VARCHAR(64)
BEGIN
  RETURN IF(is_found, NULL, CONCAT(table_name, " id=", ids, " does not exist"));
END $ DELIMITER ;
DROP FUNCTION IF EXISTS roomExists;

DELIMITER $ CREATE FUNCTION roomExists(_id INT) RETURNS BOOLEAN
BEGIN
  RETURN (SELECT COUNT(*) FROM rooms WHERE rooms.id = _id) = 1;
END $ DELIMITER ;
DROP FUNCTION IF EXISTS userExists;

DELIMITER $ CREATE FUNCTION userExists(_id INT) RETURNS BOOLEAN
BEGIN
  RETURN (SELECT COUNT(*) FROM users WHERE users.id = _id) = 1;
END $ DELIMITER ;
