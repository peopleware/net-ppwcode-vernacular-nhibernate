if object_id(N'dbo.DropTables', N'P') is not null
    DROP PROCEDURE dbo.DropTables;
GO

create procedure dbo.DropTables
as
begin
    declare
        @err int,
        @cr cursor,
        @constraint_name sysname,
        @schema_name sysname,
        @object_name sysname,
        @stmt nvarchar(1024),
        @i int,
        @recursive_fks nvarchar(4000);

    -- Drop foreign key constraints first.
    set @cr = cursor fast_forward for
        select fk.[name] as constraint_name,
               schema_name(fk.[schema_id]) as schema_name,
               object_name(fk.parent_object_id) as table_name
        from sys.foreign_keys fk;
    open @cr;
    set @err = @@error;
    while(@err = 0)
        begin
            fetch next from @cr into
                @constraint_name,
                @schema_name,
                @object_name;
            set @err = @@error;
            if (@err <> 0) or (@@FETCH_STATUS <> 0)
                break;
            set @stmt = 'alter table ['+@schema_name+'].['+@object_name+'] drop constraint '+@constraint_name;
            execute @err = sp_executesql @stmt;
        end
    close @cr;
    deallocate @cr;

    -- Drop tables
    set @cr = cursor fast_forward for
        with fk as (
            select fk.parent_object_id,
                   object_name(fk.parent_object_id) as table_name,
                   fk.referenced_object_id,
                   object_name(fk.referenced_object_id) as referenced_table_name
            from sys.foreign_keys fk
            where fk.parent_object_id <> fk.referenced_object_id)

        select '['+schema_name(o.[schema_id])+'].['+object_name(o.[object_id])+']' as [object_name]
        from sys.objects o
        where o.[type] = 'u'
          and not exists (
                select null
                from fk
                where fk.referenced_object_id = o.[object_id]);

    set @i = 0;
    while (@i < 20)
        begin
            open @cr;
            set @err = @@error;
            while(@err = 0)
                begin
                    fetch next from @cr into
                        @object_name;
                    set @err = @@error;
                    if (@err <> 0) or (@@FETCH_STATUS <> 0)
                        break;
                    set @stmt = 'drop table '+@object_name;
                    execute @err = sp_executesql @stmt;
                end
            close @cr ;
            set @i = @i + 1;
        end
    deallocate @cr;
end;
GO

EXEC	[dbo].[DropTables];
go

if object_id(N'dbo.DropTables', N'P') is not null
    DROP PROCEDURE dbo.DropTables;
GO
