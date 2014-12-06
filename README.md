Simple Sitemap Xml
==================

Sitecore package to create a the content of the sitemap.xml dynamically through a pipeline


Hi everyone again,
Today, the customer asked me for create a dynamic sitemap.xml file on the sitecore project. I tried to use some existing packages from the Marketplace, but they didn't work for this project. We were needing a very simple version. So... why not create a new one from the scratch? This is not a big deal.

We have 2 different approaches to do this:

Approach 1: We can create the sitemap.xml file and we can put it on the root of the folder. 
* Pros: Once the file is created as a static file, the indexers could take it without an extra processing of our system.
* Cons: We have to ensure that the sitemap.xml URLs are in sync with the published items. It's because the indexer could penalize this error. Maybe we can create this file when the author publishes the items to Web database. This could resolve this issue.


Approach 2: We can create the sitemap.xml content dynamically and we can show it through a handlers.
* Pros: We are recreating the file content when the indexer is trying the take it. This could ensure an accurate content all the time without penalizations.
* Cons: The system is going to process this content each time the indexer try to take it. This could take resources from our server, but we can cache this content to resolve this issue.

I want to do something super simple. I love the simple ideas, because most of the time they are more flexible on different context. So, let's take the second approach.

We can build this handler easily, adding a new pipeline on the HttpRequestBegin.

I'm sharing with you the code that I did for this package.
