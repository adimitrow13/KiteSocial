import 'package:flutter/material.dart';
import '../services/api_service.dart';
import '../models/models.dart';
import 'login_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  int _currentIndex = 0;

  final List<Widget> _pages = [
    const FeedTab(),
    const SpotsTab(),
    const LogSessionTab(),
    const ProfileTab(),
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Row(
          children: [
            Icon(Icons.surfing, color: Colors.white),
            SizedBox(width: 8),
            Text('PwrZone', style: TextStyle(fontWeight: FontWeight.bold, color: Colors.white)),
          ],
        ),
        backgroundColor: const Color(0xFF0077B6),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout, color: Colors.white),
            onPressed: () async {
              await ApiService().logout();
              if (context.mounted) {
                Navigator.of(context).pushReplacement(
                  MaterialPageRoute(builder: (_) => const LoginScreen()),
                );
              }
            },
          ),
        ],
      ),
      body: _pages[_currentIndex],
      bottomNavigationBar: NavigationBar(
        selectedIndex: _currentIndex,
        onDestinationSelected: (idx) => setState(() => _currentIndex = idx),
        destinations: const [
          NavigationDestination(icon: Icon(Icons.dynamic_feed), label: 'Фийд'),
          NavigationDestination(icon: Icon(Icons.place), label: 'Спотове'),
          NavigationDestination(icon: Icon(Icons.add_circle_outline), label: 'Нова сесия'),
          NavigationDestination(icon: Icon(Icons.person), label: 'Профил'),
        ],
      ),
    );
  }
}

// --- 1. FEED TAB ---
class FeedTab extends StatefulWidget {
  const FeedTab({super.key});

  @override
  State<FeedTab> createState() => _FeedTabState();
}

class _FeedTabState extends State<FeedTab> {
  late Future<List<Post>> _feedFuture;
  final _postController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _loadFeed();
  }

  void _loadFeed() {
    setState(() {
      _feedFuture = ApiService().getFeed();
    });
  }

  Future<void> _publishPost() async {
    if (_postController.text.trim().isEmpty) return;
    try {
      await ApiService().createPost(content: _postController.text.trim());
      _postController.clear();
      _loadFeed();
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Постът е публикуван!')));
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
    }
  }

  @override
  Widget build(BuildContext context) {
    return RefreshIndicator(
      onRefresh: () async => _loadFeed(),
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(12.0),
            child: Row(
              children: [
                Expanded(
                  child: TextField(
                    controller: _postController,
                    decoration: InputDecoration(
                      hintText: 'Споделете как духа вятърът днес...',
                      border: OutlineInputBorder(borderRadius: BorderRadius.circular(20)),
                      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                    ),
                  ),
                ),
                const SizedBox(width: 8),
                IconButton(
                  style: IconButton.styleFrom(backgroundColor: const Color(0xFF0077B6), foregroundColor: Colors.white),
                  icon: const Icon(Icons.send),
                  onPressed: _publishPost,
                ),
              ],
            ),
          ),
          Expanded(
            child: FutureBuilder<List<Post>>(
              future: _feedFuture,
              builder: (context, snapshot) {
                if (snapshot.connectionState == ConnectionState.waiting) {
                  return const Center(child: CircularProgressIndicator());
                }
                if (snapshot.hasError) {
                  return Center(child: Text('Грешка: ${snapshot.error}'));
                }
                final posts = snapshot.data ?? [];
                if (posts.isEmpty) {
                  return const Center(child: Text('Все още няма постове. Бъдете първият! 🏄‍♂️'));
                }
                return ListView.builder(
                  itemCount: posts.length,
                  itemBuilder: (context, index) {
                    final post = posts[index];
                    return Card(
                      margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                      child: Padding(
                        padding: const EdgeInsets.all(12),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Row(
                              children: [
                                CircleAvatar(
                                  backgroundColor: const Color(0xFF0077B6),
                                  child: Text(post.userName.isNotEmpty ? post.userName[0].toUpperCase() : 'K', style: const TextStyle(color: Colors.white)),
                                ),
                                const SizedBox(width: 8),
                                Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Text(post.userName, style: const TextStyle(fontWeight: FontWeight.bold)),
                                    if (post.spotName != null)
                                      Text('на ${post.spotName}', style: const TextStyle(fontSize: 12, color: Colors.grey)),
                                  ],
                                ),
                              ],
                            ),
                            const SizedBox(height: 10),
                            Text(post.content, style: const TextStyle(fontSize: 15)),
                            const SizedBox(height: 10),
                            Row(
                              children: [
                                IconButton(
                                  icon: Icon(
                                    post.isLikedByCurrentUser ? Icons.favorite : Icons.favorite_border,
                                    color: post.isLikedByCurrentUser ? Colors.red : null,
                                  ),
                                  onPressed: () async {
                                    final liked = await ApiService().toggleLike(post.id);
                                    setState(() {
                                      post.isLikedByCurrentUser = liked;
                                    });
                                  },
                                ),
                                Text('${post.likesCount}'),
                                const SizedBox(width: 16),
                                const Icon(Icons.comment_outlined, size: 20, color: Colors.grey),
                                const SizedBox(width: 4),
                                Text('${post.commentsCount}'),
                              ],
                            ),
                          ],
                        ),
                      ),
                    );
                  },
                );
              },
            ),
          ),
        ],
      ),
    );
  }
}

// --- 2. SPOTS TAB ---
class SpotsTab extends StatefulWidget {
  const SpotsTab({super.key});

  @override
  State<SpotsTab> createState() => _SpotsTabState();
}

class _SpotsTabState extends State<SpotsTab> {
  late Future<List<Spot>> _spotsFuture;

  @override
  void initState() {
    super.initState();
    _spotsFuture = ApiService().getSpots();
  }

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<List<Spot>>(
      future: _spotsFuture,
      builder: (context, snapshot) {
        if (snapshot.connectionState == ConnectionState.waiting) {
          return const Center(child: CircularProgressIndicator());
        }
        if (snapshot.hasError) {
          return Center(child: Text('Грешка: ${snapshot.error}'));
        }
        final spots = snapshot.data ?? [];
        if (spots.isEmpty) {
          return const Center(child: Text('Няма намерени спотове.'));
        }
        return ListView.builder(
          itemCount: spots.length,
          itemBuilder: (context, index) {
            final spot = spots[index];
            return ListTile(
              leading: const Icon(Icons.place, color: Color(0xFF0077B6)),
              title: Text(spot.name, style: const TextStyle(fontWeight: FontWeight.bold)),
              subtitle: Text('${spot.country}${spot.region != null ? ', ' + spot.region! : ''} • Вода: ${spot.waterTypeDisplay}'),
              trailing: Chip(label: Text('${spot.totalSessionsLogged} сесии')),
            );
          },
        );
      },
    );
  }
}

// --- 3. LOG SESSION TAB ---
class LogSessionTab extends StatefulWidget {
  const LogSessionTab({super.key});

  @override
  State<LogSessionTab> createState() => _LogSessionTabState();
}

class _LogSessionTabState extends State<LogSessionTab> {
  final _durationController = TextEditingController(text: '90');
  final _windController = TextEditingController(text: '20');
  final _kiteController = TextEditingController(text: '9');
  final _jumpController = TextEditingController(text: '8.5');
  final _notesController = TextEditingController();

  List<Spot> _spots = [];
  String? _selectedSpotId;
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _loadSpots();
  }

  Future<void> _loadSpots() async {
    try {
      final list = await ApiService().getSpots();
      setState(() {
        _spots = list;
        if (list.isNotEmpty) _selectedSpotId = list.first.id;
      });
    } catch (_) {}
  }

  Future<void> _saveSession() async {
    if (_selectedSpotId == null) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Моля изберете спот!')));
      return;
    }
    setState(() => _isLoading = true);
    try {
      await ApiService().logSession(
        spotId: _selectedSpotId!,
        sessionDate: DateTime.now(),
        durationMinutes: int.tryParse(_durationController.text) ?? 60,
        windSpeedKnots: double.tryParse(_windController.text) ?? 18.0,
        kiteSize: double.tryParse(_kiteController.text) ?? 9.0,
        boardType: 1,
        maxJumpHeightMeters: double.tryParse(_jumpController.text),
        notes: _notesController.text,
      );
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Сесията е записана успешно! 🤙')));
      }
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Запиши кайт сесия', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold)),
          const SizedBox(height: 16),
          if (_spots.isNotEmpty)
            DropdownButtonFormField<String>(
              value: _selectedSpotId,
              decoration: const InputDecoration(labelText: 'Спот', border: OutlineInputBorder()),
              items: _spots.map((s) => DropdownMenuItem(value: s.id, child: Text('${s.name} (${s.country})'))).toList(),
              onChanged: (val) => setState(() => _selectedSpotId = val),
            ),
          const SizedBox(height: 12),
          TextField(controller: _durationController, decoration: const InputDecoration(labelText: 'Минути във водата', border: OutlineInputBorder()), keyboardType: TextInputType.number),
          const SizedBox(height: 12),
          TextField(controller: _windController, decoration: const InputDecoration(labelText: 'Вятър (възли / knots)', border: OutlineInputBorder()), keyboardType: TextInputType.number),
          const SizedBox(height: 12),
          TextField(controller: _kiteController, decoration: const InputDecoration(labelText: 'Размер на крило (кв. м)', border: OutlineInputBorder()), keyboardType: TextInputType.number),
          const SizedBox(height: 12),
          TextField(controller: _jumpController, decoration: const InputDecoration(labelText: 'Най-висок скок WOO (метри)', border: OutlineInputBorder()), keyboardType: TextInputType.number),
          const SizedBox(height: 12),
          TextField(controller: _notesController, decoration: const InputDecoration(labelText: 'Бележки за сесията', border: OutlineInputBorder()), maxLines: 2),
          const SizedBox(height: 20),
          _isLoading
              ? const Center(child: CircularProgressIndicator())
              : SizedBox(
                  width: double.infinity,
                  height: 48,
                  child: ElevatedButton(
                    style: ElevatedButton.styleFrom(backgroundColor: const Color(0xFF0077B6), foregroundColor: Colors.white),
                    onPressed: _saveSession,
                    child: const Text('Запази сесията', style: TextStyle(fontSize: 16)),
                  ),
                ),
        ],
      ),
    );
  }
}

// --- 4. PROFILE TAB ---
class ProfileTab extends StatelessWidget {
  const ProfileTab({super.key});

  @override
  Widget build(BuildContext context) {
    final user = ApiService().currentUser;
    return FutureBuilder<Map<String, dynamic>>(
      future: ApiService().getSessionStats(),
      builder: (context, snapshot) {
        final stats = snapshot.data ?? {};
        return Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            children: [
              const CircleAvatar(radius: 40, backgroundColor: Color(0xFF0077B6), child: Icon(Icons.person, size: 50, color: Colors.white)),
              const SizedBox(height: 8),
              Text(user?.fullName ?? 'Kiter', style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold)),
              Text('@${user?.username ?? ""}', style: const TextStyle(color: Colors.grey)),
              const SizedBox(height: 20),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceAround,
                children: [
                  _statCard('Сесии', '${stats['totalSessions'] ?? 0}'),
                  _statCard('Часове', '${((stats['totalMinutesOnWater'] ?? 0) / 60).toStringAsFixed(1)}ч'),
                  _statCard('Рекорд скок', '${stats['maxJumpHeightRecord'] ?? 0}m'),
                ],
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _statCard(String label, String value) {
    return Card(
      elevation: 3,
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
        child: Column(
          children: [
            Text(value, style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold, color: Color(0xFF0077B6))),
            const SizedBox(height: 4),
            Text(label, style: const TextStyle(fontSize: 13, color: Colors.grey)),
          ],
        ),
      ),
    );
  }
}
