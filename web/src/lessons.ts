import overviewInstructions from './content/overview/instructions.md?raw';
import overviewDocs from './content/overview/docs.md?raw';
import documentsInstructions from './content/documents/instructions.md?raw';
import documentsDocs from './content/documents/docs.md?raw';
import sqliteInstructions from './content/sqlite/instructions.md?raw';
import sqliteDocs from './content/sqlite/docs.md?raw';
import chunkingInstructions from './content/chunking/instructions.md?raw';
import chunkingDocs from './content/chunking/docs.md?raw';
import embeddingsInstructions from './content/embeddings/instructions.md?raw';
import embeddingsDocs from './content/embeddings/docs.md?raw';
import retrievalInstructions from './content/retrieval/instructions.md?raw';
import retrievalDocs from './content/retrieval/docs.md?raw';
import answerInstructions from './content/answer/instructions.md?raw';
import answerDocs from './content/answer/docs.md?raw';
import projectMap from './content/project-map.md?raw';

export type LessonId =
  | 'overview'
  | 'documents'
  | 'sqlite'
  | 'chunking'
  | 'embeddings'
  | 'retrieval'
  | 'answer';

export type Lesson = {
  id: LessonId;
  title: string;
  instructions: string;
  docs: string;
};

export const lessons: Lesson[] = [
  { id: 'overview', title: '1. Project overview', instructions: overviewInstructions, docs: overviewDocs },
  { id: 'documents', title: '2. Documents & ingestion', instructions: documentsInstructions, docs: documentsDocs },
  { id: 'sqlite', title: '3. SQLite basics', instructions: sqliteInstructions, docs: sqliteDocs },
  { id: 'chunking', title: '4. Chunking', instructions: chunkingInstructions, docs: chunkingDocs },
  { id: 'embeddings', title: '5. Embeddings', instructions: embeddingsInstructions, docs: embeddingsDocs },
  { id: 'retrieval', title: '6. Retrieval', instructions: retrievalInstructions, docs: retrievalDocs },
  { id: 'answer', title: '7. Answer generation', instructions: answerInstructions, docs: answerDocs },
];

export const upcomingLessons = [
  '8. Hybrid search',
  '9. Reranking',
  '10. Query transformation',
  '11. Metadata filtering',
  '12. Streaming & memory',
  '13. Vector index (optional)',
];

export { projectMap };
