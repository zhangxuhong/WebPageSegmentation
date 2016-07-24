# Web Page Segmentation


## Description

In the training part, after workers confirm bid detailed in shadow area,  Add new process for marking bids tittle, bid number, due date, etc. The labeling process is directed by a NEXT menu item. Corresponding functions used can be found in the NEXT menu click event. Here, we store into database the path of current html element to the main block element on the new DOM tree. 

In the extraction part, goes directly to the part where hash value for bids details on a web page is calculated. If the hash value is new, then use the path marked in the training part to extract bid details

In the Method.cs, there is a RetrieveElement function to extract the target node.