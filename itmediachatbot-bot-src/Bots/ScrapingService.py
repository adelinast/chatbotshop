# Documentation available on https://www.kdnuggets.com/2018/02/web-scraping-tutorial-python.html
from bs4 import BeautifulSoup
import requests

class ScrapingService:
    def EMAGScraping(self, page_link):
    
        #fetch the content from url
        page_response = requests.get(page_link, timeout=5)
        page_content = BeautifulSoup(page_response.content, "html.parser")

        # extract all html elements from page
        prices = page_content.find_all(attrs={'class':'product-new-price'});
        aux = str(next(iter(prices)));
        i1 = aux.index('<sup')
        #aux2 = aux1[i1+17:]
        i2 = aux.index('>')
        result = aux[i2+1:i1];
        return float(result);

    def CELScraping(self, page_link):
    
        #fetch the content from url
        page_response = requests.get(page_link, timeout=5)
        page_content = BeautifulSoup(page_response.content, "html.parser")

        # extract all html elements from page
        prices = page_content.find_all(attrs={'class':'productPrice'});
        aux1 = str(prices)
        i1 = aux1.index('productprice="1">')
        aux2 = aux1[i1+17:]
        i2 = aux2.index('<')
        result = aux2[0:i2];
        return float(result);

    def FLANCOScraping(self, page_link):
        #fetch the content from url
        page_response = requests.get(page_link, timeout=5)
        page_content = BeautifulSoup(page_response.content, "html.parser")

        # extract all html elements from page
        prices = page_content.find_all(attrs={'class':'produs-price'});
        #print(prices);
        aux1 = str(prices)
        i1 = aux1.index('content="')
        aux2 = aux1[i1+9:]
        i2 = aux2.index('" ')
        result = aux2[0:i2];
        return float(result);

