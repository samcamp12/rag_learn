import { useState } from 'react';
import { LeftSidebar } from './components/LeftSidebar';
import { RightPanel, type RightTab } from './components/RightPanel';
import { AnswerLesson } from './pages/AnswerLesson';
import { ChunkingLesson } from './pages/ChunkingLesson';
import { DocumentsLesson } from './pages/DocumentsLesson';
import { EmbeddingsLesson } from './pages/EmbeddingsLesson';
import { OverviewLesson } from './pages/OverviewLesson';
import { RetrievalLesson } from './pages/RetrievalLesson';
import { SqliteLesson } from './pages/SqliteLesson';
import { lessons, type LessonId } from './lessons';

export default function App() {
  const [activeId, setActiveId] = useState<LessonId>('overview');
  const [rightTab, setRightTab] = useState<RightTab>('instructions');

  const lesson = lessons.find((l) => l.id === activeId)!;

  return (
    <div className="layout">
      <LeftSidebar activeId={activeId} onSelect={setActiveId} />

      <main className="main">
        <h2 className="lesson-title">{lesson.title}</h2>
        {activeId === 'overview' && <OverviewLesson />}
        {activeId === 'documents' && <DocumentsLesson />}
        {activeId === 'sqlite' && <SqliteLesson />}
        {activeId === 'chunking' && <ChunkingLesson />}
        {activeId === 'embeddings' && <EmbeddingsLesson />}
        {activeId === 'retrieval' && <RetrievalLesson />}
        {activeId === 'answer' && <AnswerLesson />}
      </main>

      <RightPanel lesson={lesson} tab={rightTab} onTabChange={setRightTab} />
    </div>
  );
}
