-- CREATE DATABASE bids;
USE bids;

DROP TABLE IF EXISTS pdf_urls;
DROP TABLE IF EXISTS sample_links;
DROP TABLE IF EXISTS hashes;
DROP TABLE IF EXISTS alerts;
DROP TABLE IF EXISTS alarms;
DROP TABLE IF EXISTS pages;

CREATE TABLE pages (
  id INT AUTO_INCREMENT NOT NULL PRIMARY KEY,
  -- path_to_bid_region TEXT NOT NULL,
  page_url TEXT NOT NULL,

  link_prefix_length INT NOT NULL,

  sample_link TEXT NOT NULL,

  last_checked TIMESTAMP NULL DEFAULT NULL,

  -- is_locked BOOL NOT NULL, /* ? */

  when_locked TIMESTAMP NULL, /* ? */
  locked_by_pid INT NULL,

  needs_retraining BOOL NOT NULL

) ENGINE = InnoDB;

/*
CREATE TABLE sample_links (
  id INT AUTO_INCREMENT NOT NULL PRIMARY KEY,
  
  page_id INT NOT NULL,

  link TEXT NOT NULL,

  CONSTRAINT FOREIGN KEY
    sample_links(id) REFERENCES pages(id)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT

) ENGINE = InnoDb;
*/

CREATE TABLE hashes (
  id INT AUTO_INCREMENT PRIMARY KEY,

  page_id INT NOT NULL,

  -- hash VARBINARY(32) NOT NULL,
  hash VARCHAR(32) NOT NULL, -- longer than hash strings, but that's OK

  status ENUM('none', 'unseen', 'seen', 'dead'),

  url TEXT NULL DEFAULT NULL, /* we actually *do* need this */
 
  -- last_seen DATE NULL DEFAULT NULL, /* presently unused */

  -- changed from "alert" to "alarmed", for consistency with
  --   field name "detected".
  alarmed TIMESTAMP NULL DEFAULT NULL,

  detected TIMESTAMP NOT NULL,

  CONSTRAINT FOREIGN KEY
    hashes(page_id) REFERENCES pages(id)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT,

  -- Life will go on if we drop this constraint entirely.
  CONSTRAINT UNIQUE INDEX
    (page_id, hash, url(512))

) ENGINE = innodb;
-- using unique index over the first 512 columns of the url field

/*
CREATE TABLE alerts (
  id INT AUTO_INCREMENT PRIMARY KEY,

  
) ENGINE = innodb;
*/

/*
CREATE TABLE pdf_urls (
  id INT AUTO_INCREMENT NOT NULL PRIMARY KEY,

  parent_page INT NOT NULL,

  pdf_url TEXT NOT NULL,
  pdf_hash BINARY(16) NOT NULL,
--  pdf_hash VARBINARY NOT NULL,

  CONSTRAINT FOREIGN KEY
    pdf_urls(parent_page) REFERENCES pages(id)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT

--  CONSTRAINT PRIMARY KEY id

) ENGINE = innodb;
*/
