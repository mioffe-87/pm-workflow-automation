from google.cloud import bigquery

client = bigquery.Client(project="itp-aia-datalake")

dataset_id = "itp-aia-datalake.csp"
print(f"--- Tables in {dataset_id} ---")
tables = client.list_tables(dataset_id)
for table in tables:
    print(f"Table: {table.table_id}")
