CREATE OR REPLACE FUNCTION sys_scheme.del_field()
  RETURNS trigger AS
$BODY$DECLARE
cnt integer;
BEGIN
	IF (SELECT style_field FROM sys_scheme.table_info WHERE id = OLD.id_table) = OLD.name_db
	THEN UPDATE sys_scheme.table_info SET default_style = TRUE, style_field = 'style' WHERE id = OLD.id_table;
	END IF;
	IF (SELECT range_column FROM sys_scheme.table_info WHERE id = OLD.id_table) = OLD.name_db
	THEN UPDATE sys_scheme.table_info SET range_column = NULL, range_colors = FALSE WHERE id = OLD.id_table;
	END IF;
	IF (SELECT lablefiled FROM sys_scheme.table_info WHERE id = OLD.id_table) like '%(('||OLD.name_db||')::text)%' 
	THEN UPDATE sys_scheme.table_info SET label_showlabel = FALSE, lablefiled = replace(lablefiled, '(('||OLD.name_db||')::text)', '(('''||OLD.name_db||''')::text)')  WHERE id = OLD.id_table;
	END IF;
	UPDATE sys_scheme.table_info SET view_name = sys_scheme.create_view_for_table(OLD.id_table) WHERE id = OLD.id_table;
	RETURN NEW;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  
  
INSERT INTO sys_scheme.db_version (major, minor, build, revision, version_seq, date_update) VALUES (2, 8, 4, 0, 2, now());