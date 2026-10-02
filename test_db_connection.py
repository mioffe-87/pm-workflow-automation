import mysql.connector
import os
from dotenv import load_dotenv

# Load environment variables from .env
load_dotenv()

# Get connection details from .env
config = {
    'host': os.getenv('MYSQL_HOST'),
    'user': os.getenv('MYSQL_USER'),
    'password': os.getenv('MYSQL_PASSWORD'),
    'database': os.getenv('MYSQL_DATABASE'),
    'port': int(os.getenv('MYSQL_PORT', 3306))
}

print(f"Attempting to connect to {config['host']}:{config['port']} as {config['user']}...")

try:
    connection = mysql.connector.connect(**config)
    
    if connection.is_connected():
        db_info = connection.get_server_info()
        print(f"✅ Successfully connected to MySQL Server version {db_info}")
        
        # Test query
        cursor = connection.cursor()
        cursor.execute("SELECT DATABASE();")
        result = cursor.fetchone()
        print(f"✅ Current database: {result[0]}")
        
        cursor.close()
        connection.close()
        print("✅ Connection test passed!")
    
except mysql.connector.Error as err:
    if err.errno == 2003:
        print(f"❌ Error: Cannot connect to MySQL server at '{config['host']}'")
        print("   Check if the host is reachable and the port is correct")
    elif err.errno == 1045:
        print(f"❌ Error: Access denied for user '{config['user']}'")
        print("   Check your username and password")
    else:
        print(f"❌ Error: {err}")
except Exception as e:
    print(f"❌ Unexpected error: {e}")
