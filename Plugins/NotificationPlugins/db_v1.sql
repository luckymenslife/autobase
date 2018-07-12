CREATE SCHEMA notification;

GRANT ALL ON SCHEMA notification TO sm;
GRANT ALL ON SCHEMA notification TO public;

ALTER DEFAULT PRIVILEGES IN SCHEMA notification GRANT INSERT, SELECT, UPDATE, DELETE, TRUNCATE, REFERENCES, TRIGGER ON TABLES TO public;
ALTER DEFAULT PRIVILEGES IN SCHEMA notification GRANT SELECT, UPDATE, USAGE ON SEQUENCES TO public;
ALTER DEFAULT PRIVILEGES IN SCHEMA notification GRANT EXECUTE ON FUNCTIONS TO public;
ALTER DEFAULT PRIVILEGES IN SCHEMA notification GRANT USAGE ON TYPES TO public;

-- #############################################################################

CREATE TABLE notification.notification_priority_types
(
  gid serial NOT NULL,
  typename character varying,
  weight integer,
  knowncolor integer,
  CONSTRAINT notification_priority_types_pkey PRIMARY KEY (gid)
);

-- #################

CREATE TABLE notification.notification_types
(
  gid serial NOT NULL,
  typename character varying,
  CONSTRAINT notification_types_pkey PRIMARY KEY (gid)
);

-- #################

CREATE TABLE notification.notifications
(
  gid serial NOT NULL,
  type_gid integer,
  priority_type_gid integer,
  subject character varying,
  message character varying,
  created timestamp with time zone,
  CONSTRAINT notifications_pkey PRIMARY KEY (gid),
  CONSTRAINT notifications_priority_type_gid_fkey FOREIGN KEY (priority_type_gid)
      REFERENCES notification.notification_priority_types (gid) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION,
  CONSTRAINT notifications_type_gid_fkey FOREIGN KEY (type_gid)
      REFERENCES notification.notification_types (gid) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
);

-- #################

CREATE TABLE notification.notification_users
(
  notification_gid integer NOT NULL,
  user_gid integer NOT NULL,
  seen_datetime timestamp with time zone,
  favorite boolean DEFAULT false,
  CONSTRAINT notification_users_pkey PRIMARY KEY (notification_gid, user_gid),
  CONSTRAINT notification_users_notification_gid_fkey FOREIGN KEY (notification_gid)
      REFERENCES notification.notifications (gid) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION,
  CONSTRAINT notification_users_user_gid_fkey FOREIGN KEY (user_gid)
      REFERENCES sys_scheme.user_db (id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
);

-- #################

CREATE TABLE notification.notification_tableinfo
(
  table_gid integer,
  row_gid integer,
  notification_gid integer NOT NULL,
  CONSTRAINT notification_tableinfo_pkey PRIMARY KEY (notification_gid),
  CONSTRAINT notification_tableinfo_notification_gid_fkey FOREIGN KEY (notification_gid)
      REFERENCES notification.notifications (gid) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION,
  CONSTRAINT notification_tableinfo_table_gid_fkey FOREIGN KEY (table_gid)
      REFERENCES sys_scheme.table_info (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
);

-- #################

CREATE TABLE notification.notification_counter
(
  user_gid integer NOT NULL,
  cnt_unread integer,
  cnt_favorite integer,
  CONSTRAINT notification_counter_pkey PRIMARY KEY (user_gid),
  CONSTRAINT notification_counter_user_gid_fkey FOREIGN KEY (user_gid)
      REFERENCES sys_scheme.user_db (id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
);

-- #############################################################################

CREATE OR REPLACE FUNCTION notification.get_current_user_id()
  RETURNS integer AS
$BODY$
DECLARE
	_id_user integer;
BEGIN
	SELECT id INTO _id_user FROM sys_scheme.user_db WHERE login = session_user;
	RETURN _id_user;
END;
$BODY$
  LANGUAGE plpgsql;

-- #################

CREATE OR REPLACE FUNCTION notification.get_unreadnotification_count()
/* ### УСТАРЕЛО !!! ### */
  RETURNS integer AS
$BODY$
DECLARE
	_id_user integer;
	_count integer;
BEGIN
	SELECT * INTO _id_user FROM notification.get_current_user_id();

   SELECT count(*) INTO _count FROM notification.notification_users WHERE seen_datetime is null and user_gid = _id_user;

	RETURN _count;
END;
$BODY$
  LANGUAGE plpgsql;

-- #################

CREATE OR REPLACE FUNCTION notification.get_count_unread()
  RETURNS integer AS
$BODY$
DECLARE
	_id_user integer;
	_count integer;
BEGIN
	SELECT * INTO _id_user FROM notification.get_current_user_id();

   SELECT cnt_unread INTO _count FROM notification.notification_counter WHERE user_gid = _id_user LIMIT 1;

	RETURN _count;
END;
$BODY$
  LANGUAGE plpgsql;

-- #################

CREATE OR REPLACE FUNCTION notification.get_count_favorite()
  RETURNS integer AS
$BODY$
DECLARE
	_id_user integer;
	_count integer;
BEGIN
	SELECT * INTO _id_user FROM notification.get_current_user_id();

   SELECT cnt_favorite INTO _count FROM notification.notification_counter WHERE user_gid = _id_user LIMIT 1;

	RETURN _count;
END;
$BODY$
  LANGUAGE plpgsql;

-- #################

CREATE OR REPLACE FUNCTION notification.update_all_counter()
  RETURNS void AS
$BODY$
BEGIN
   -- Подчистим
	DELETE FROM notification.notification_counter;-- WHERE user_gid not in (SELECT user_gid FROM notification.notification_users GROUP BY user_gid);
	
   -- Зальем
	INSERT INTO notification.notification_counter(user_gid, cnt_unread, cnt_favorite)
      SELECT
         user_gid,
         sum(case when seen_datetime is null then 1 else 0 end),
         sum(case when favorite = true then 1 else 0 end)
      FROM notification.notification_users
      GROUP BY user_gid;

END;
$BODY$
  LANGUAGE plpgsql;

-- #################

CREATE OR REPLACE FUNCTION notification.create_notification(
    _type_gid integer,
    _priority_type_gid integer,
    _subject character varying,
    _message character varying,
    _user_gid integer,
    _table_gid integer,
    _row_gid integer)
  RETURNS integer AS
$BODY$
DECLARE
	 _new_gid INTEGER;
BEGIN
   SELECT nextval('notification.notifications_gid_seq') INTO _new_gid;

   INSERT INTO notification.notifications (gid, type_gid, priority_type_gid, subject, message, created)
      VALUES (_new_gid, _type_gid, _priority_type_gid, _subject, _message, date_trunc('second', current_timestamp));

   INSERT INTO notification.notification_users(notification_gid, user_gid, seen_datetime)
      VALUES (_new_gid, _user_gid, null);
   IF (_table_gid is not null OR (_table_gid is not null AND _row_gid is not null)) THEN
      INSERT INTO notification.notification_tableinfo(table_gid, row_gid, notification_gid)
         VALUES (_table_gid, _row_gid, _new_gid);
   END IF;

   RETURN 0;
EXCEPTION
   WHEN OTHERS THEN
      RETURN -1;
END;
$BODY$
  LANGUAGE plpgsql;

-- #############################################################################

CREATE OR REPLACE VIEW notification.view_all_notifications AS 
 SELECT n.gid,
    n.type_gid,
    t.typename AS type_name,
    n.priority_type_gid,
    pt.typename AS priority_name,
    pt.weight,
    n.subject,
    n.message,
    n.created,
    u.seen_datetime,
    i.table_gid,
    i.row_gid,
    u.user_gid,
    u.favorite
   FROM notification.notification_users u,
    notification.notification_types t,
    notification.notification_priority_types pt,
    notification.notifications n
     LEFT JOIN notification.notification_tableinfo i ON n.gid = i.notification_gid
  WHERE n.gid = u.notification_gid AND n.type_gid = t.gid AND n.priority_type_gid = pt.gid;

-- #################

CREATE OR REPLACE VIEW notification.view_user_notifications AS 
 SELECT n.gid,
    n.type_gid,
    t.typename AS type_name,
    n.priority_type_gid,
    pt.typename AS priority_name,
    pt.weight,
    n.subject,
    n.message,
    n.created,
    u.seen_datetime,
    i.table_gid,
    i.row_gid,
    u.user_gid,
    u.favorite
   FROM notification.notification_users u,
    notification.notification_types t,
    notification.notification_priority_types pt,
    notification.notifications n
     LEFT JOIN notification.notification_tableinfo i ON n.gid = i.notification_gid
  WHERE n.gid = u.notification_gid AND n.type_gid = t.gid AND n.priority_type_gid = pt.gid AND u.user_gid = (( SELECT get_current_user_id.get_current_user_id
           FROM notification.get_current_user_id() get_current_user_id(get_current_user_id)));

-- #################

CREATE OR REPLACE FUNCTION notification.upd_counter()
  RETURNS trigger AS
$BODY$
DECLARE
   have_user integer;
BEGIN
   
   IF (TG_OP = 'DELETE') THEN
      IF (OLD.seen_datetime is null) THEN
         UPDATE notification.notification_counter SET cnt_unread = cnt_unread - 1 WHERE user_gid = OLD.user_gid;
      END IF;

      IF (OLD.favorite = true) THEN
         UPDATE notification.notification_counter SET cnt_favorite = cnt_favorite - 1 WHERE user_gid = OLD.user_gid;
      END IF;
      
      RETURN OLD;
   ELSIF (TG_OP = 'UPDATE') THEN
   
      IF (OLD.seen_datetime is null AND NEW.seen_datetime is not null) THEN
         UPDATE notification.notification_counter SET cnt_unread = cnt_unread - 1 WHERE user_gid = NEW.user_gid;
      ELSIF (OLD.seen_datetime is not null AND NEW.seen_datetime is null) THEN
         UPDATE notification.notification_counter SET cnt_unread = cnt_unread + 1 WHERE user_gid = NEW.user_gid;
      END IF;

      IF (OLD.favorite = false AND NEW.favorite = true) THEN
         UPDATE notification.notification_counter SET cnt_favorite = cnt_favorite + 1 WHERE user_gid = NEW.user_gid;
      ELSIF (OLD.favorite = true AND NEW.favorite = false) THEN
         UPDATE notification.notification_counter SET cnt_favorite = cnt_favorite - 1 WHERE user_gid = NEW.user_gid;
      END IF;
      
      RETURN NEW;
   ELSIF (TG_OP = 'INSERT') THEN

      SELECT count(*) INTO have_user FROM notification.notification_counter WHERE user_gid = NEW.user_gid;
      IF (have_user is null OR have_user <= 0) THEN
         INSERT INTO notification.notification_counter(user_gid, cnt_unread, cnt_favorite) VALUES (NEW.user_gid, 0, 0);
      END IF;

      IF (NEW.seen_datetime is null) THEN
         UPDATE notification.notification_counter SET cnt_unread = cnt_unread + 1 WHERE user_gid = NEW.user_gid;
      END IF;

      IF (NEW.favorite = true) THEN
         UPDATE notification.notification_counter SET cnt_favorite = cnt_favorite + 1 WHERE user_gid = NEW.user_gid;
      END IF;
      
      RETURN NEW;
   END IF;
   
   RETURN NULL;
END;$BODY$
  LANGUAGE plpgsql;

CREATE TRIGGER notification_users_upd_counter_trg
  AFTER INSERT OR UPDATE OR DELETE
  ON notification.notification_users
  FOR EACH ROW
  EXECUTE PROCEDURE notification.upd_counter();

  -- #################

