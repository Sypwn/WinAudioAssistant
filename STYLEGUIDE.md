# Project Coding Standards

## Naming Conventions
* PascalCase for all class names, method names, and property names.
* _camelCase for private fields.
* camelCase for local variables and method arguments.

Names should be detailed and descriptive. Avoid abbreviations and acronyms.

## Class Structure
Classes should be generally organized in the following order:
1. Constructors
2. Private fields and properties
3. Public properties
4. Public events
5. Public methods
6. Private methods
7. Interface methods
8. Destructor
9. Internal types (enums, structs, etc.)

#region directives should be used for classes and structs exceeding 100 lines of code.  
More specific #regions can be created for related fields/properties/methods.

## Commenting and Documentation
* Properties and fields should have inline comments explaining their purpose.  
Groups of related fields or properties with a common purpose can have a single comment above the group.  
Properties with complex behavior in their accessors may warrant full XML documentation.  
* Methods, events, classes, and other types should have XML documentation.  
For event handler and command methods, <param> tags may be ommitted.  
* Complex or unclear behavior within a method should include comments.  