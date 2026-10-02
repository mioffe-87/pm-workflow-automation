from google.cloud import bigquery

client = bigquery.Client(project="itp-aia-datalake")

with open("schema_context.md", "w") as f:
    f.write("# Dataset Schema: itp-aia-datalake.csp\n\n")
    for table_item in client.list_tables("itp-aia-datalake.csp"):
        table = client.get_table(table_item.reference)
        f.write(f"## Table: {table.table_id}\n")
        f.write("| Column Name | Type | Mode |\n|---|---|---|\n")
        for schema_field in table.schema:
            f.write(f"| {schema_field.name} | {schema_field.field_type} | {schema_field.mode} |\n")
        f.write("\n")
print("Saved schema context to schema_context.md!")
