# Umbraco and OpenAI Chatbot Integration

Welcome to the official repository for the Umbraco and OpenAI chatbot integration session at Duugfest 2023!

![Chatbot Demo](https://github.com/FrontisBV/DF23-UmbracoOpenAIChatBot/assets/16189228/3c7e9454-152b-48e5-a299-da6c3c812df3)

## Session Description

In this session, we'll explore the powerful fusion of Umbraco and OpenAI Functions to create an AI-driven chatbot. Discover how the latest AI models can intelligently respond to user requests with JSON outputs, enabling dynamic content generation for Umbraco pages. We'll showcase the chatbot in action, demonstrating its ability to create pages on various topics.

### Module Features

- **Message History**: Message history is saved automatically.
- **Custom Storage Logic**: Message history available for custom storing logic.
- **Recursive OpenAI Calls**: OpenAI functions calls are handled recursively.
- **Extensibility**: Custom function calls can be added by adding a single file.
- **User-Friendly GUI**: Chatbot GUI available.
- **Optimized Communication**: Automapper usage for downscaling the Umbraco models (Content & ContentType) to optimize tokenization, resulting in faster communication.

### Chatbot Umbraco Features

- **Retrieve Information**: Retrieve document type information and content item information.
- **Create Content**: Create content items (pages).
- **Update Content**: Update content items (pages).

### Current Limitations

- Document type permissions are not taken into account when creating pages.
- Multi-culture support is missing.
- Message history is saved into memory by default.
- Integration tests are not implemented.
- Injecting custom function calls is not yet supported.
- Builder pattern does not support custom configuration.
- Tokenization can grow beyond the token limits.

## Getting Started

To get started with this project, please follow the installation and usage instructions:

1. Create or use a valid OpenAI Api key that needs to be filled in the appsettings.json

```
  "OpenAIServiceOptions": {
    "ApiKey": "sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
  }
```

2. It runs in IISExpress on this url https://localhost:44389/umbraco

## Contributing

We welcome contributions from the community. If you'd like to contribute to this project, please check out our [Contribution Guidelines](CONTRIBUTING.md).

## License

This project is licensed under the [MIT License](LICENSE.md).

## Contact

If you have any questions or need assistance, feel free to contact us:

- Email: [info@frontis.nl](info@frontis.nl)
- Github: [@FrontisBV](https://github.com/FrontisBV/)

## Acknowledgments

We would like to express our gratitude to the Umbraco and OpenAI communities for their invaluable contributions and support.

Thank you for joining us at Duugfest 2023!

Happy Coding! 🚀

