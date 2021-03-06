CREATE OR REPLACE FUNCTION sys_scheme.trg_after_fts_field_delete()
  RETURNS trigger AS
$BODY$
DECLARE
	count int;
BEGIN
	SELECT count(*)	INTO count FROM sys_scheme.fts_fields WHERE id_table = OLD.id_table;

	IF count = 0 THEN
		DELETE FROM sys_scheme.fts_fields WHERE id_table = OLD.id_table;
	END IF;
	RETURN OLD;
END; 
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;

CREATE OR REPLACE FUNCTION sys_scheme.get_sql_create_view(id_table_val integer)
  RETURNS character varying AS
$BODY$DECLARE
table_name_val character varying;
scheme_name_val character varying;

index_tab_alias integer;
alias_string character varying;
alias_string2 character varying;
sql_string character varying;
select_string character varying;
from_string character varying;

label_string character varying;

rec_field sys_scheme.table_field_info;
pk_field_val text;

BEGIN
sql_string:='';
select_string:='SELECT ';
index_tab_alias:=1;
SELECT scheme_name, name_db, lablefiled, pk_fileld INTO scheme_name_val, table_name_val, label_string, pk_field_val FROM sys_scheme.table_info WHERE id = id_table_val;

if label_string is null OR label_string='' THEN
	label_string:='null';
END IF;


from_string:='FROM "'||scheme_name_val||'"."'||table_name_val||'" ';
--raise notice '1:: %', from_string;
FOR rec_field IN (SELECT id, id_table, name_db, name_map, type_field, visible, name_lable, 
			       is_reference, is_interval, is_style, ref_table, ref_field, ref_field_end, 
			       ref_field_name, num_order
			  FROM sys_scheme.table_field_info
			  WHERE id_table = id_table_val AND visible = TRUE ORDER BY num_order) LOOP
--raise notice '2:: %', select_string;
	IF rec_field.is_reference=FALSE AND rec_field.is_interval = FALSE THEN
		select_string:=select_string ||'"'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'" '|| 
						'as '||rec_field.name_db||', ';
	ELSIF rec_field.is_reference=TRUE THEN
		alias_string:='___v$'||index_tab_alias;
		select_string:=select_string||alias_string||'."'||sys_scheme.get_table_field_name(rec_field.ref_field_name)||'"
				as '||rec_field.name_db||', ';
		select_string:=select_string ||'"'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'" '|| 
						'as "_RealValue_'||rec_field.name_db||'", ';
		from_string:= from_string|| 'LEFT JOIN '||'"'||sys_scheme.get_table_scheme_name(rec_field.ref_table)||
					'"."'||sys_scheme.get_table_name(rec_field.ref_table)||
					'" '||alias_string||' ON "'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'"='||
							alias_string||'."'||sys_scheme.get_table_field_name(rec_field.ref_field)||'" ';
	index_tab_alias:=index_tab_alias+1;
	ELSIF rec_field.is_interval=TRUE THEN
		alias_string:='___v$'||index_tab_alias;
		index_tab_alias:=index_tab_alias+1;
		alias_string2:='___v$'||index_tab_alias;
		select_string:=select_string || alias_string||'."'||sys_scheme.get_table_field_name(rec_field.ref_field_name)||'"||''(''||"'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'"||'')'' '||
		'as '||rec_field.name_db||', ';
		select_string:=select_string ||'"'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'" '|| 
						'as "_RealValue_'||rec_field.name_db||'", ';
		from_string:=from_string|| 'LEFT JOIN '||'"'||sys_scheme.get_table_scheme_name(rec_field.ref_table)||
					'"."'||sys_scheme.get_table_name(rec_field.ref_table)||
					'" '||alias_string||' ON '||alias_string||'."'||sys_scheme.get_table_pkfield(rec_field.ref_table)||
					'" = (SELECT '||alias_string2||'."'||sys_scheme.get_table_pkfield(rec_field.ref_table)||
					'" FROM "'||sys_scheme.get_table_scheme_name(rec_field.ref_table)||
					'"."'||sys_scheme.get_table_name(rec_field.ref_table)||
					'" '||alias_string2||' WHERE "'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'">'||alias_string2||
					'."'||sys_scheme.get_table_field_name(rec_field.ref_field)||'" AND 
					"'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'"<='||alias_string2||
					'."'||sys_scheme.get_table_field_name(rec_field.ref_field_end)||'" LIMIT 1) ';
	index_tab_alias:=index_tab_alias+1;
	END IF;
--raise notice 'SELECT:: %', select_string;
--raise notice 'FROM:: %', from_string;
	
END LOOP;
--raise notice 'SELECT_STOP:: %', select_string;

select_string:=select_string||'(SELECT ('||label_string||') FROM "'||scheme_name_val||'"."'||table_name_val||'" tforlabel WHERE tforlabel."'||pk_field_val||'" = "'||scheme_name_val||'"."'||table_name_val||'"."'||pk_field_val||'" ) as "_ResultLabel_"';
--select_string:=substring(select_string from 0 for char_length(select_string)-1);
sql_string:=select_string||' '||from_string;
RETURN sql_string;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  
CREATE OR REPLACE FUNCTION sys_scheme.update_db_from_2_10_0_0_to_2_10_1_0()
  RETURNS boolean AS
$BODY$DECLARE
exists_version_new boolean;
exists_version_old boolean;
major_val_new integer;
minor_val_new integer;
build_val_new integer;
revision_val_new integer;
major_val_old integer;
minor_val_old integer;
build_val_old integer;
revision_val_old integer;
version_seq_new integer;
BEGIN
	major_val_new:=2;
	minor_val_new:=10;
	build_val_new:=1;
	revision_val_new:=0;
	
	version_seq_new:=6;
	
	major_val_old:=2;
	minor_val_old:=10;
	build_val_old:=0;
	revision_val_old:=0;
	
		select exists(SELECT true FROM sys_scheme.db_version WHERE major = major_val_new 
						AND minor = minor_val_new 
						AND build=build_val_new 
						AND revision =revision_val_new) INTO exists_version_new;
		IF exists_version_new = true THEN
			RAISE EXCEPTION '���������: %', '��� ���������� ��� ����!';
			return false;
		END IF;
		
		select exists(SELECT true FROM sys_scheme.db_version WHERE major = major_val_old 
						AND minor = minor_val_old 
						AND build=build_val_old 
						AND revision =revision_val_old) INTO exists_version_old;
		IF exists_version_old = false THEN
			RAISE EXCEPTION '���������: %', '���������� ������� �������� �� ������ 2.9.1!';
			return false;
		END IF;
		
		raise notice '���������: %', '��������� ���� ����������';
		INSERT INTO sys_scheme.db_version(major, minor, build, revision, version_seq) VALUES (major_val_new, minor_val_new, build_val_new, revision_val_new, version_seq_new);
		
		raise notice '���������: %', '���������� ������ �������!';
		return true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;

SELECT sys_scheme.update_db_from_2_10_0_0_to_2_10_1_0();
  -- ������ ���� ��  
SELECT sys_scheme.super_fix_db();