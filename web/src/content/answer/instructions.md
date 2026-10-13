# 7. Answer generation

Now the language model writes the answer. The backend retrieves the top chunks, places them in the prompt as numbered sources, and asks the model to answer only from those sources and to cite them.

## Steps

1. Complete lessons 5 and 6 so the chunks are embedded.
2. In the **Ask** panel, type a question such as `What is chunk overlap and why is it used?`.
3. Click **Ask**. Wait a few seconds for the model.
4. Read the answer. Citations like `[1]` refer to the **Sources** list below it.
5. Open a source and check that the answer actually follows from it.
6. Ask something the documents do not cover, such as `What is the capital of France?`. The model should say it does not know.

## What to notice

- The answer is only as good as the sources. If retrieval returns the wrong chunks, the answer will be wrong.
- The **Sources** list is the same list the model saw. Compare them to check citations.
- Changing **Top K** changes how much context the model receives.
- Each request costs a small amount of tokens. The model is `gpt-4o-mini` by default.
