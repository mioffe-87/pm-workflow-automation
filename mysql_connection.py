#!/usr/bin/env python3
"""
MySQL Connection Module
Provides utilities for connecting to and querying MySQL databases
"""

import os
import sys
from typing import Optional, List, Dict, Any
import pandas as pd

# MySQL connector options
try:
    import mysql.connector
    from mysql.connector import Error
    MYSQL_CONNECTOR_AVAILABLE = True
except ImportError:
    MYSQL_CONNECTOR_AVAILABLE = False
    print("⚠️  mysql-connector-python not installed. Install with: pip install mysql-connector-python")

try:
    import pymysql
    PYMYSQL_AVAILABLE = True
except ImportError:
    PYMYSQL_AVAILABLE = False

try:
    from sqlalchemy import create_engine
    SQLALCHEMY_AVAILABLE = True
except ImportError:
    SQLALCHEMY_AVAILABLE = False


class MySQLConnection:
    """
    MySQL connection wrapper supporting multiple drivers
    """
    
    def __init__(
        self,
        host: str,
        user: str,
        password: str,
        database: str,
        port: int = 3306,
        driver: str = "mysql-connector"  # Options: "mysql-connector", "pymysql", "sqlalchemy"
    ):
        """
        Initialize MySQL connection
        
        Args:
            host: MySQL server hostname
            user: Database username
            password: Database password
            database: Database name
            port: MySQL port (default: 3306)
            driver: Connection driver to use
        """
        self.host = host
        self.user = user
        self.password = password
        self.database = database
        self.port = port
        self.driver = driver
        self.connection = None
        self.cursor = None
    
    def connect(self) -> bool:
        """
        Establish connection to MySQL database
        
        Returns:
            bool: True if connection successful, False otherwise
        """
        try:
            if self.driver == "mysql-connector":
                if not MYSQL_CONNECTOR_AVAILABLE:
                    raise ImportError("mysql-connector-python not installed")
                
                self.connection = mysql.connector.connect(
                    host=self.host,
                    user=self.user,
                    password=self.password,
                    database=self.database,
                    port=self.port
                )
                self.cursor = self.connection.cursor()
                print(f"✅ Connected to MySQL via mysql-connector: {self.host}:{self.port}/{self.database}")
                return True
            
            elif self.driver == "pymysql":
                if not PYMYSQL_AVAILABLE:
                    raise ImportError("pymysql not installed")
                
                self.connection = pymysql.connect(
                    host=self.host,
                    user=self.user,
                    password=self.password,
                    database=self.database,
                    port=self.port
                )
                self.cursor = self.connection.cursor()
                print(f"✅ Connected to MySQL via pymysql: {self.host}:{self.port}/{self.database}")
                return True
            
            elif self.driver == "sqlalchemy":
                if not SQLALCHEMY_AVAILABLE:
                    raise ImportError("sqlalchemy not installed")
                
                connection_string = f"mysql+pymysql://{self.user}:{self.password}@{self.host}:{self.port}/{self.database}"
                self.engine = create_engine(connection_string)
                self.connection = self.engine.connect()
                print(f"✅ Connected to MySQL via SQLAlchemy: {self.host}:{self.port}/{self.database}")
                return True
            
            else:
                raise ValueError(f"Unknown driver: {self.driver}")
        
        except Error as e:
            print(f"❌ MySQL Connection Error: {e}")
            return False
        except Exception as e:
            print(f"❌ Unexpected Error: {e}")
            return False
    
    def disconnect(self) -> None:
        """Close database connection"""
        if self.connection:
            if self.cursor:
                self.cursor.close()
            self.connection.close()
            print("✅ Disconnected from MySQL")
    
    def execute_query(self, query: str) -> Optional[List[Dict[str, Any]]]:
        """
        Execute SELECT query and return results
        
        Args:
            query: SQL SELECT query
            
        Returns:
            List of dictionaries with query results
        """
        if not self.connection:
            print("❌ Not connected to database")
            return None
        
        try:
            self.cursor.execute(query)
            columns = [desc[0] for desc in self.cursor.description]
            results = []
            for row in self.cursor.fetchall():
                results.append(dict(zip(columns, row)))
            return results
        except Error as e:
            print(f"❌ Query Error: {e}")
            return None
    
    def execute_update(self, query: str) -> bool:
        """
        Execute INSERT/UPDATE/DELETE query
        
        Args:
            query: SQL INSERT/UPDATE/DELETE query
            
        Returns:
            bool: True if successful
        """
        if not self.connection:
            print("❌ Not connected to database")
            return False
        
        try:
            self.cursor.execute(query)
            self.connection.commit()
            print(f"✅ Query executed. Rows affected: {self.cursor.rowcount}")
            return True
        except Error as e:
            self.connection.rollback()
            print(f"❌ Query Error: {e}")
            return False
    
    def query_to_dataframe(self, query: str) -> Optional[pd.DataFrame]:
        """
        Execute query and return results as pandas DataFrame
        
        Args:
            query: SQL SELECT query
            
        Returns:
            pandas DataFrame with query results
        """
        if not self.connection:
            print("❌ Not connected to database")
            return None
        
        try:
            if self.driver == "sqlalchemy":
                df = pd.read_sql(query, self.connection)
            else:
                results = self.execute_query(query)
                if results:
                    df = pd.DataFrame(results)
                else:
                    df = pd.DataFrame()
            return df
        except Exception as e:
            print(f"❌ Error converting to DataFrame: {e}")
            return None
    
    def get_tables(self) -> Optional[List[str]]:
        """Get list of tables in database"""
        query = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = %s"
        try:
            self.cursor.execute(query, (self.database,))
            tables = [row[0] for row in self.cursor.fetchall()]
            return tables
        except Error as e:
            print(f"❌ Error fetching tables: {e}")
            return None


# ============================================================================
# QUICK START EXAMPLES
# ============================================================================

def example_basic_connection():
    """Example: Basic connection and query"""
    print("\n" + "="*80)
    print("EXAMPLE 1: Basic Connection and Query")
    print("="*80)
    
    # Configuration
    config = {
        'host': 'localhost',
        'user': 'root',
        'password': 'your_password',
        'database': 'your_database',
        'port': 3306
    }
    
    # Connect
    db = MySQLConnection(**config)
    if not db.connect():
        return
    
    # Execute query
    query = "SELECT * FROM users LIMIT 5"
    results = db.execute_query(query)
    
    if results:
        print(f"\n✅ Retrieved {len(results)} rows:")
        for row in results:
            print(row)
    
    db.disconnect()


def example_dataframe_query():
    """Example: Query to pandas DataFrame"""
    print("\n" + "="*80)
    print("EXAMPLE 2: Query to Pandas DataFrame")
    print("="*80)
    
    config = {
        'host': 'localhost',
        'user': 'root',
        'password': 'your_password',
        'database': 'your_database'
    }
    
    db = MySQLConnection(**config)
    if not db.connect():
        return
    
    query = """
    SELECT 
        user_id,
        email,
        created_at,
        last_login
    FROM users
    WHERE created_at > DATE_SUB(NOW(), INTERVAL 30 DAY)
    ORDER BY created_at DESC
    """
    
    df = db.query_to_dataframe(query)
    if df is not None:
        print(f"\n✅ Retrieved {len(df)} rows:")
        print(df.head(10))
        print(f"\nDataFrame Info:")
        print(df.info())
    
    db.disconnect()


def example_environment_variables():
    """Example: Using environment variables for credentials"""
    print("\n" + "="*80)
    print("EXAMPLE 3: Using Environment Variables")
    print("="*80)
    
    # Set these in your environment or .env file:
    # export MYSQL_HOST=localhost
    # export MYSQL_USER=root
    # export MYSQL_PASSWORD=your_password
    # export MYSQL_DATABASE=your_database
    
    config = {
        'host': os.getenv('MYSQL_HOST', 'localhost'),
        'user': os.getenv('MYSQL_USER', 'root'),
        'password': os.getenv('MYSQL_PASSWORD', ''),
        'database': os.getenv('MYSQL_DATABASE', 'test'),
        'port': int(os.getenv('MYSQL_PORT', 3306))
    }
    
    print(f"\n📋 Configuration:")
    print(f"   Host: {config['host']}")
    print(f"   User: {config['user']}")
    print(f"   Database: {config['database']}")
    print(f"   Port: {config['port']}")
    
    db = MySQLConnection(**config)
    if db.connect():
        tables = db.get_tables()
        if tables:
            print(f"\n📊 Available tables: {', '.join(tables)}")
        db.disconnect()


if __name__ == "__main__":
    print("🔧 MySQL Connection Module")
    print("Run examples with: python mysql_connection.py")
    
    # Uncomment to run examples:
    # example_basic_connection()
    # example_dataframe_query()
    # example_environment_variables()
