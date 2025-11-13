# Blackbird.io Uniform

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

Uniform is a composable Digital Experience Platform (DXP) designed for enterprise digital marketing teams. It allows users to create omnichannel digital experiences by integrating various content, data, and technology sources.

## Before setting up

Before you connect your Uniform instance to Blackbird, make sure you have the following:

- A Uniform instance with access to team settings
- Team admin permissions to create API keys
- Project ID for the project you want to work with

### How to create an API key

1. Open your team page in Uniform at https://uniform.app
2. Navigate to **Security > API Keys**
3. Click **Add API key**
4. Enter a name for your API key (note: you cannot change the name after creation)
5. Configure permissions:
   - For most uses, select **Role assignment** and inherit permissions from existing roles
   - For CLI or CI/CD operations, you may need **Team admin** access
   - Alternatively, assign **Custom permissions** for specific project needs
6. Click **Create API Key**
7. Copy and save the following values (they will only be displayed once):
   - **Key**: Your API key
   - **Project ID**: The project identifier
   - **Quick Connect**: A combined string with all connection details

> **Warning**: The API key and Quick Connect values are only displayed once. Save them securely before closing the dialog.

More details can be found in the [Uniform documentation](https://docs.uniform.app/docs/guides/api-keys).

## Connecting

1. Navigate to apps and search for **Uniform**
2. Click _Add Connection_
3. Name your connection for future reference e.g., 'My Uniform connection'
4. Fill in the following fields:
   - **API Key**: Your Uniform API key (see previous section)
   - **Project ID**: Your Uniform project ID
5. Click _Connect_
6. Confirm that the connection has appeared and the status is _Connected_

![Connection](image/README/connection.png)

## Actions

### Entries

- **Search entries**: Retrieve a list of entries based on specified criteria
- **Get entry**: Retrieve details of a specific entry by its ID
- **Download entry**: Download entry as HTML file for translation
- **Upload entry**: Update entry from translated HTML file
- **Delete entry**: Deletes or unpublishes an entry by its ID

### Compositions

- **Search compositions**: Retrieve a list of compositions based on specified criteria
- **Get composition**: Retrieve details of a specific composition by its ID
- **Download composition**: Download composition as HTML file for translation
- **Upload composition**: Update composition from translated HTML file
- **Delete composition**: Deletes or unpublishes a composition by its ID

## Events

### Entry Events

- **On entries created**: Polling event that periodically checks for newly created entries
- **On entries updated**: Polling event that periodically checks for updated entries
- **On entries published**: Polling event that periodically checks for newly published or updated published entries

### Composition Events

- **On compositions created**: Polling event that periodically checks for newly created compositions
- **On compositions updated**: Polling event that periodically checks for updated compositions

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->
