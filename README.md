# trurl

Automaton of the universe, constructor of worlds, and friend to all

Trurl consists of a client which loads plugin assemblies via DI which implement an interface.
* `ICommand` - Executes an action in response to a user request
* `IJob` - Executes an action in response to a schedule
* `IEvent` - Executes an action in resopnse to an event (filesystem, webhook, etc)


## TODO

* Plugin versioning
* Ability to load plugins from arbitrary locations
* Possibility of more than one communication system (e.g IRC, Hipchat, etc)
* Support multiple "channels", not just a single channel
* Implement `IJob` and a scheduling system 
* Implement `IEvent` and associated handlers
