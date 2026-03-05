This service receives a file or a body containing an array of json objects.
Without overloading the memory, it will validate the correct json objects and store them into a file, whilst discarding the invalid json objects.

Default Limits by Server for atachments 
Kestrel/IIS/HTTP.sys	~28.6 MB

While there is no requirments to process large amount of data (> 100 mb), so I will not
use "fire-and-forget" way to process data (it could be overengeneering),
to process 30 mb with this approach should be okay.

Assumptions:
	if files attached - only one file should be attached (otherwise request will be discarded)


Ways to improve:
	1. For large files, we can use "fire-and-forget" approach so that
		a) return response to client immediately
		b) process data on background (it could be even parallelized by chunks, but it exceeds requirments for now)
	2. It could be good to return some information about invalid json objects (e.g. line number, error message) to client
	3. It would be good to know how json could be correputed, is there any schema for original data etc.
