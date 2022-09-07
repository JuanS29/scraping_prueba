import requests
from bs4 import BeautifulSoup

session = requests.session()
url = 'https://www.google.com/recaptcha/api2/reload?k=6LfTRbUZAAAAAC4PZ6QF3Oz_IElVRZpnW9zzA7JD'

headersaa = {
    'Content-Type': 'application/json',
    'referer': 'https://www.google.com/'
}
session = requests.Session()
print(session.cookies.get_dict())
response = session.post('https://www.google.com/recaptcha/api2/reload?k=6LfTRbUZAAAAAC4PZ6QF3Oz_IElVRZpnW9zzA7JD')
print(session.cookies.get_dict())
request_data = {"filtro": {"nombreProducto": "PARACETAMOL", "pagina": 1,"tamanio": 10}}
raw = requests.post(url)
response = requests.post(url, headers=headersaa)
print("Status Code", response.status_code)
print(raw.text)