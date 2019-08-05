from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.chrome.options import Options
from bs4 import BeautifulSoup # library used export data from web content
import time
import pyodbc

def Login(url):
    ###########################################################

    #enter the link to the website you want to automate login.
    website_link_true="https://www.sunnyportal.com/Templates/Start.aspx?logout=true"
    #enter your login username
    username_true="monitoring@ers.my"
    #enter your login password
    password_true="ers12345"

    ###########################################################

    #enter the element for username input field
    element_for_username="ctl00$ContentPlaceHolder1$Logincontrol1$txtUserName"
    #enter the element for password input field
    element_for_password="ctl00$ContentPlaceHolder1$Logincontrol1$txtPassword"
    #enter the element for submit button
    element_for_submit="ctl00$ContentPlaceHolder1$Logincontrol1$LoginBtn"

    ###########################################################

    chrome_options = Options()  
    #chrome_options.add_argument("--headless")  
    

    #browser = webdriver.Chrome("C:/Users/ersenergy/Desktop/Khee_Intern/chromedriver.exe",chrome_options=chrome_options)
    browser = webdriver.PhantomJS("C:/Users/ersenergy/Desktop/Khee_Intern/PythonWebScraper/PythonWebScraper/phantomjs-2.1.1-windows/bin/phantomjs.exe")
    browser.get((website_link_true))

    username = browser.find_element_by_name(element_for_username)
    username.send_keys(username_true)		
    password  = browser.find_element_by_name(element_for_password)
    password.send_keys(password_true)
    signInButton = browser.find_element_by_name(element_for_submit)
    signInButton.click()

    browser.get(url) #redirect dashboard URL depends on target
    time.sleep(1); #delay for webpage to load fully
    res = browser.execute_script("return document.documentElement.outerHTML")    
    browser.quit()

    # parser the web content
    soup = BeautifulSoup(res, 'html.parser')
    data_list = soup.find_all('div',{'class':'mainValue'})
    data_list_string = ""
    for string in data_list:
        data_list_string += str(string)
    
    #TIMESTAMP
    timestamp = data_list[0]
    timestamp = str(timestamp).split("\n")
    timestamp = timestamp[3]
    timestamp = timestamp.split("data-timestamp")
    timestamp = timestamp[1][2:21] 
    #MM/dd/YYYY
    timestamp_format = "'" + timestamp + "'"

    #SQL START
    #server = "sqlsever-ers.database.windows.net"
    #database = "inverterDB"
    #username = "ers"
    #password = "testing123#"
    #driver = '{ODBC Driver 17 for SQL Server}'
    #cnxn = pyodbc.connect('DRIVER='+driver+';SERVER='+server+';PORT=1433;DATABASE='+database+';UID='+username+';PWD='+ password)
    #cursor = cnxn.cursor()
    #connection_string = "INSERT INTO SUNNY_PORTAL_STRING (_datetime, html_str) VALUES (%s, '%s')" %  (timestamp_format, data_list_string)
    #cursor.execute(connection_string)

    connection_string = "INSERT INTO SUNNY_PORTAL_STRING (_datetime, html_str) VALUES (%s, '%s')" %  (timestamp_format, data_list_string)
    return (connection_string)



Login("https://www.sunnyportal.com/RedirectToPlant/50f57782-7818-40b9-8455-72d2a96ae8ea")