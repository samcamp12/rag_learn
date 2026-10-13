# What is RAG?

Retrieval-Augmented Generation (RAG) is a pattern for answering questions with a language model using your own documents. Instead of relying only on what the model learned during training, the system first retrieves relevant passages from a knowledge base and then passes them to the model as context.

## Why RAG exists

Large language models have a knowledge cutoff and do not know your private documents. They can also produce confident but wrong answers, which is called hallucination. RAG reduces both problems by grounding the answer in text you can show to the user.

## The basic pipeline

A RAG system has two main phases:

1. Ingestion: load documents, split them into chunks, convert each chunk into an embedding, and store the chunks in a database.
2. Query: convert the user's question into an embedding, find the most similar chunks, and send those chunks together with the question to a chat model.

Each phase can be improved independently. Better chunking, better embeddings, better ranking, and better prompts all affect the final answer quality.

## Measuring quality

A RAG system is only as good as its retrieval. If the right chunk is never retrieved, the model cannot answer correctly no matter how capable it is. That is why this project includes an evaluation lesson later on.
